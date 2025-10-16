using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using System.Diagnostics;

namespace MyBlog.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILoggerService _loggerService;

        public ErrorController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [Route("Error/{statusCode}")]
        public async Task<IActionResult> HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            switch (statusCode)
            {
                case 404:
                    await _loggerService.LogInfoAsync($"404 Not Found - Path: {statusCodeResult?.OriginalPath}",
                        userId != null ? int.Parse(userId) : null);
                    return View("NotFound");
                case 403:
                    await _loggerService.LogInfoAsync($"403 Forbidden - Path: {statusCodeResult?.OriginalPath}",
                        userId != null ? int.Parse(userId) : null);
                    return View("AccessDenied");
                default:
                    await _loggerService.LogInfoAsync($"{statusCode} Error - Path: {statusCodeResult?.OriginalPath}",
                        userId != null ? int.Parse(userId) : null);
                    return View("Error");
            }
        }

        [Route("Error")]
        public async Task<IActionResult> Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (exceptionHandlerPathFeature?.Error != null)
            {
                await _loggerService.LogErrorAsync("Unhandled exception in application",
                    exceptionHandlerPathFeature.Error,
                    userId != null ? int.Parse(userId) : null);
            }

            return View("Error");
        }
    }
}