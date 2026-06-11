namespace Lesson3_CNLTWeb.Middleware
{
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

            // chỉ ghi log với CRUD Book, lọc bỏ log các path bootstrap, jquery, css, js
            if (path.Contains("/Book"))
            {
                Console.WriteLine($"[Time before action: {time}] Method: {method} - Path: {path}");
            }
            
            if (path == "/Book/Detail/0" || path == "/Book/Detail/-1")
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Book id khong hop le");
                return;
            }

            await _next(context);

            if (path.Contains("/Book"))
            {
                var timeAfter = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                Console.WriteLine($"Time After action: [{timeAfter}] - Method: {method} - Path: {path}");
            }

                
        }
    }
}
