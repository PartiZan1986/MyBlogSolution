using Microsoft.AspNetCore.Mvc;

namespace MyBlog.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Страница не найдена";
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Внутренняя ошибка сервера";
                    break;
                default:
                    ViewBag.ErrorMessage = "Произошла ошибка";
                    break;
            }
            return View("Error");
        }
    }
}
