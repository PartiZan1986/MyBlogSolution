using MyBlog.Core.Interfaces;
using System.Threading.Tasks;

namespace MyBlog.Web.Middleware
{
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerService _loggerService;

        public ErrorLoggingMiddleware(RequestDelegate next, ILoggerService loggerService)
        {
            _next = next;
            _loggerService = loggerService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                await _loggerService.LogErrorAsync("Unhandled exception in middleware", ex,
                    userId != null ? int.Parse(userId) : null);
                throw;
            }
        }
    }

    public static class ErrorLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorLoggingMiddleware>();
        }
    }
}
