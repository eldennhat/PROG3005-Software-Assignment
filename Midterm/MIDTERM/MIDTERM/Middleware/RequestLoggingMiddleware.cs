namespace MIDTERM.Middleware
{
    /// <summary>
    /// Intercepts requests to log structural metrics for Dish processing and filter bad ID endpoints.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var timeBefore = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var method = context.Request.Method;
            var path = context.Request.Path.ToString();

            // Filter logging scope specifically to capture metadata metrics for the new Dish flow
            if (path.Contains("/Dish", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[Execution Metric - Start: {timeBefore}] Method: {method} - Target Routing Path: {path}");
            }
            
            if (path.StartsWith("/Dish/", StringComparison.OrdinalIgnoreCase))
            {
                var segments = path.Split('/');
                // Example path split structure: ["", "Dish", "Detail", "0"]
                if (segments.Length > 3 && int.TryParse(segments[3], out int parsedId) && parsedId <= 0)
                {
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    await context.Response.WriteAsync("Requirement 6 Failure: Target Identification ID is structurally invalid.");
                    return;
                }
            }

            // Forward the active context safely down the standard pipeline stack
            await _next(context);

            if (path.Contains("/Dish", StringComparison.OrdinalIgnoreCase))
            {
                var timeAfter = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine($"[Execution Metric - Complete: {timeAfter}] Method: {method} - Target Routing Path: {path}");
            }
        }
    }
}