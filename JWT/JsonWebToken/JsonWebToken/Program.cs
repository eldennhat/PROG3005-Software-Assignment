using JsonWebToken.Data;
using JsonWebToken.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC
builder.Services.AddControllersWithViews();

//Đăng ký session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Dùng để gọi API Cloudflare Turnstile
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Missing Jwt:Key configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Missing Jwt:Issuer configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
                  ?? throw new InvalidOperationException("Missing Jwt:Audience configuration.");

// JWT is used by API clients. MVC login still uses Session below, so the existing UI flow is unchanged.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Enables [Authorize] and [Authorize(Roles = "...")] on Web API endpoints.
builder.Services.AddAuthorization();

// Lấy chuỗi kết nối từ appsettings.Development.json hoặc appsettings.json
var connectionString = builder.Configuration
                           .GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException(
                           "Không tìm thấy ConnectionStrings:DefaultConnection."
                       );

// Đăng ký ApplicationDbContext và cấu hình SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Kiểm tra kết nối database khi khởi động ứng dụng
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    try
    {
        var connected = await dbContext.Database.CanConnectAsync();

        Console.WriteLine(
            connected
                ? "Kết nối MovieTicketDB thành công!"
                : "Không thể kết nối MovieTicketDB!"
        );
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Lỗi kết nối database: {exception.Message}");
    }
}

// Cấu hình HTTP request pipeline
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        await SeedJwtTestUsersAsync(dbContext);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Loi seed JWT test users: {exception.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// API clients send JWTs with: Authorization: Bearer <token>.
// Authentication must run before authorization so role policies can read claims from the token.
app.UseAuthentication();

app.UseAuthorization();

// Định tuyến MVC
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task SeedJwtTestUsersAsync(ApplicationDbContext dbContext)
{
    await UpsertTestUserAsync(
        dbContext,
        fullName: "Admin Test",
        email: "bac@gmail.com",
        password: "Admin@123",
        role: "Admin");

    await UpsertTestUserAsync(
        dbContext,
        fullName: "User Test",
        email: "user@gmail.com",
        password: "User@123",
        role: "KhachHang");
}

static async Task UpsertTestUserAsync(
    ApplicationDbContext dbContext,
    string fullName,
    string email,
    string password,
    string role)
{
    var normalizedEmail = email.Trim().ToLowerInvariant();
    var user = await dbContext.Users
        .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);

    if (user == null)
    {
        dbContext.Users.Add(new User
        {
            FullName = fullName,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Status = true
        });
    }
    else
    {
        user.FullName = fullName;
        user.Role = role;
        user.Status = true;

        // Repair old manually inserted test users whose PasswordHash contains
        // plain text/dev placeholders. Login still verifies BCrypt only.
        if (IsInvalidTestPasswordHash(user.PasswordHash))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    await dbContext.SaveChangesAsync();
}

static bool IsInvalidTestPasswordHash(string? passwordHash)
{
    return string.IsNullOrWhiteSpace(passwordHash)
           || passwordHash is "User@123" or "Admin@123" or "DEVELOPMENT_PASSWORDLESS_ACCOUNT"
           || !passwordHash.StartsWith("$2", StringComparison.Ordinal);
}
