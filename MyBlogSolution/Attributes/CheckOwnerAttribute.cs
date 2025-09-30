using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyBlog.Core.Interfaces;
using System.Security.Claims;

namespace MyBlog.Web.Attributes
{
    public class CheckOwnerAttribute : ActionFilterAttribute
    {
        private readonly string _entityType;

        public CheckOwnerAttribute(string entityType = "article")
        {
            _entityType = entityType.ToLower();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Админы и модераторы имеют доступ ко всему
            if (user.HasClaim(ClaimTypes.Role, "Admin") || user.HasClaim(ClaimTypes.Role, "Moderator"))
            {
                await next();
                return;
            }

            // Проверяем владельца для разных типов сущностей
            switch (_entityType)
            {
                case "article":
                    await CheckArticleOwner(context, userId);
                    break;
                case "comment":
                    await CheckCommentOwner(context, userId);
                    break;
                case "user":
                    await CheckUserOwner(context, userId);
                    break;
            }

            await next();
        }

        private async Task CheckArticleOwner(ActionExecutingContext context, string currentUserId)
        {
            if (context.ActionArguments.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out int articleId))
            {
                var articleService = context.HttpContext.RequestServices.GetService<IArticleService>();
                var article = await articleService.GetArticleByIdAsync(articleId);

                if (article != null && article.AuthorId.ToString() != currentUserId)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                }
            }
        }

        private async Task CheckCommentOwner(ActionExecutingContext context, string currentUserId)
        {
            if (context.ActionArguments.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out int commentId))
            {
                var commentService = context.HttpContext.RequestServices.GetService<ICommentService>();
                var comment = await commentService.GetCommentByIdAsync(commentId);

                if (comment != null && comment.AuthorId.ToString() != currentUserId)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                }
            }
        }

        private async Task CheckUserOwner(ActionExecutingContext context, string currentUserId)
        {
            if (context.ActionArguments.TryGetValue("id", out var idObj) && int.TryParse(idObj?.ToString(), out int userId))
            {
                if (userId.ToString() != currentUserId)
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                }
            }
        }
    }
}
