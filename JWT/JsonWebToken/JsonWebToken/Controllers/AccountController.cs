namespace JsonWebToken.Controllers;

using JsonWebToken.Data;
using JsonWebToken.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AccountController : Controller
{
    // Đọc cấu hình từ appsettings.json
    // Ví dụ: Cloudflare Turnstile SiteKey và SecretKey
    private readonly IConfiguration _configuration;

    // Dùng để tạo HttpClient gửi request tới Cloudflare
    // DbContext dùng để truy vấn và lưu dữ liệu trong database
    private readonly ApplicationDbContext _context;


    // Dependency Injection:
    // ASP.NET Core tự động truyền các service cần thiết vào Controller
    public AccountController(
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    // =====================================================
    // HIỂN THỊ TRANG ĐĂNG NHẬP
    // GET: /Account/Login
    // =====================================================
    [HttpGet]
    public IActionResult Login()
    {
        // Lấy SiteKey của Cloudflare Turnstile
        // và truyền sang View thông qua ViewBag
        return View();
    }

    // =====================================================
    // XỬ LÝ FORM ĐĂNG NHẬP MVC
    // POST: /Account/Login
    // =====================================================
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        // Khi trả lại View do có lỗi, View vẫn cần SiteKey
        // để hiển thị lại CAPTCHA        

        // Lấy token CAPTCHA do Cloudflare Turnstile gửi từ form

        // CaptchaToken không được gửi bằng asp-for,
        // nên loại thuộc tính này khỏi ModelState
        // và kiểm tra CAPTCHA riêng ở phía dưới


        // Kiểm tra các validation của Email và Password
        // trong LoginRequest
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Gửi token CAPTCHA tới Cloudflare để xác thực

        // Chuẩn hóa email trước khi tìm kiếm
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == email);


        // Nếu không tìm thấy tài khoản hoặc mật khẩu không đúng
        // thì trả về cùng một thông báo để tránh lộ email tồn tại
        if (user == null || !IsPasswordValid(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Sai email hoặc mật khẩu");
            TempData["AlertError"] = "Sai email hoặc mật khẩu. Vui lòng thử lại.";
            return View(model);
        }

        // Kiểm tra trạng thái tài khoản
        // Status = false nghĩa là tài khoản đã bị khóa
        if (!user.Status)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa");
            TempData["AlertError"] = "Tài khoản của bạn đã bị khóa.";
            return View(model);
        }


        // Đăng nhập thành công — Lưu thông tin user vào Session
        var role = user.Role; // Xử lý null Role cho user cũ trong DB

        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", role);
        HttpContext.Session.SetInt32("UserID", user.UserID);

        // Thiết lập thông báo thành công
        TempData["AlertSuccess"] = $"Đăng nhập thành công! Xin chào {user.FullName} (Role: {role})";

        // Phân quyền chuyển hướng theo Role
        return role switch
        {
            "Admin" => RedirectToAction("Index", "Admin"),
            "Staff" => RedirectToAction("Index", "Staff"),
            _ => RedirectToAction("Index", "Home")  // KhachHang hoặc mặc định
        };


    }


    // =====================================================
    // API ĐĂNG NHẬP
    // POST: /login
    //
    // Login flow for API clients:
    // 1. Validate request body.
    // 2. Find the account by email and verify the BCrypt password hash.
    // 3. Check Status so locked accounts cannot receive tokens.
    // 4. Return a short-lived JWT access token. The client sends it as:
    //    Authorization: Bearer <JWT>
    //
    // Request body:
    // {
    //   "email": "admin@example.com",
    //   "password": "Admin@123"
    // }
    // =====================================================
    [HttpPost("login")]
    public async Task<IActionResult> LoginApi([FromBody] ApiLoginRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { message = "Invalid request body." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (user == null || !IsPasswordValid(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Sai email hoac mat khau" });
        }

        if (!user.Status)
        {
            return Unauthorized(new { message = "Tai khoan da bi khoa" });
        }

        var (token, expiresAt, apiRole) = GenerateJwtToken(user);

        return Ok(new
        {
            message = "Dang nhap thanh cong",
            token,
            expiresAt,
            user = new
            {
                user.UserID,
                user.FullName,
                user.Email,
                role = apiRole
            }
        });
    }

    // =====================================================
    // XỬ LÝ ĐĂNG XUẤT
    // GET: /Account/Logout
    // =====================================================
    [HttpGet]
    public IActionResult Logout()
    {
        // Xóa toàn bộ thông tin trong Session
        HttpContext.Session.Clear();

        // Xóa thông báo (nếu có)
        TempData["AlertSuccess"] = "Bạn đã đăng xuất thành công.";

        // Đẩy về trang chủ
        return RedirectToAction("Index", "Home");
    }

    // =====================================================
    // HIỂN THỊ TRANG ĐĂNG KÝ
    // GET: /Account/Register
    // =====================================================
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AuthViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Chuẩn hóa email
        var email = model.Email.Trim().ToLowerInvariant();

        // Kiểm tra email đã tồn tại
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email);

        if (emailExists)
        {
            AddDuplicateAccountError();
            return View(model);
        }

        // Chuyển từ ViewModel sang User Entity
        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = email,
            PhoneNumber = model.PhoneNumber.Trim(),
            DOB = model.DateOfBirth,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Status = true,
            Role = "KhachHang"
        };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (IsDuplicateAccountError(exception))
        {
            AddDuplicateAccountError();
            return View(model);
        }

        TempData["SuccessMessage"] = "Đăng ký thành công. Hãy đăng nhập.";

        return RedirectToAction(nameof(Login));
    }

    // Logic xu ly luu database tao tai khoan

    private void AddDuplicateAccountError()
    {
        ModelState.AddModelError(nameof(AuthViewModel.Email), "Tài khoản đã có");
    }

    private static bool IsDuplicateAccountError(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
               && sqlException.Errors
                   .Cast<SqlError>()
                   .Any(error => error.Number is 2601 or 2627);
    }

    private static bool IsPasswordValid(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    private (string Token, DateTime ExpiresAt, string Role) GenerateJwtToken(User user)
    {
        var key = _configuration["Jwt:Key"]
                  ?? throw new InvalidOperationException("Missing Jwt:Key configuration.");
        var issuer = _configuration["Jwt:Issuer"]
                     ?? throw new InvalidOperationException("Missing Jwt:Issuer configuration.");
        var audience = _configuration["Jwt:Audience"]
                       ?? throw new InvalidOperationException("Missing Jwt:Audience configuration.");
        var expireMinutes = _configuration.GetValue("Jwt:ExpireMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);
        var apiRole = GetApiRole(user.Role);

        // Keep JWT payload small: identity, email, display name, and role only.
        // JWT authorization reads ClaimTypes.Role. Customer accounts use the
        // database role "KhachHang" because the DB CHECK constraint does not allow "User".
        // PasswordHash and other sensitive/account data must never be stored in the token.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, apiRole)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt, apiRole);
    }

    private static string GetApiRole(string? role)
    {
        // Keep role names exactly as stored in the database: Admin, Staff, KhachHang.
        // Normal customers are KhachHang, not User, because of the database role constraint.
        if (string.IsNullOrWhiteSpace(role))
        {
            return "KhachHang";
        }

        return role.Trim();
    }

}
