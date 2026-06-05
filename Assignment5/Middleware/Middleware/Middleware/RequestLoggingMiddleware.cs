namespace Middleware.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var method = context.Request.Method;
        var path = context.Request.Path.ToString();

        Console.WriteLine($"[{time}] Method: {method} - Path: {path}");

        if (path == "/Book/Detail/0" || path == "/Book/Detail/-1")
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid book ID");
            return;
        }

        await _next(context);

        Console.WriteLine($"Status Code: {context.Response.StatusCode}");
    }
}