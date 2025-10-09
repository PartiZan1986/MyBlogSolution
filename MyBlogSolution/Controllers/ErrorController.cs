using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MyBlog.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Запрашиваемая страница не найдена";
                    ViewBag.OriginalPath = statusCodeResult?.OriginalPath;
                    ViewBag.OriginalQueryString = statusCodeResult?.OriginalQueryString;
                    return View("NotFound");
                case 403:
                    ViewBag.ErrorMessage = "Доступ к запрашиваемому ресурсу запрещен";
                    return View("AccessDenied");
                default:
                    ViewBag.ErrorMessage = $"Произошла ошибка {statusCode}";
                    return View("Error");
            }
        }

        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            ViewBag.ErrorMessage = exceptionHandlerPathFeature?.Error.Message ?? "Произошла непредвиденная ошибка";
            ViewBag.Path = exceptionHandlerPathFeature?.Path;
            ViewBag.StackTrace = exceptionHandlerPathFeature?.Error.StackTrace;

            // Логирование ошибки (в реальном приложении)
            // _logger.LogError(exceptionHandlerPathFeature.Error, "Произошла ошибка в приложении");

            return View("Error");
        }
    }
}