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
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Проверка аутентификации
            if (string.IsNullOrEmpty(currentUserId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // ✅ Админы и модераторы имеют доступ ко всему - пропускаем проверку
            if (user.IsInRole("Admin") || user.IsInRole("Moderator"))
            {
                await next();
                return;
            }

            // ✅ Проверяем владельца для разных типов сущностей
            var hasAccess = _entityType switch
            {
                "article" => await CheckArticleOwnerAsync(context, currentUserId),
                "comment" => await CheckCommentOwnerAsync(context, currentUserId),
                "user" => await CheckUserOwnerAsync(context, currentUserId),
                _ => true
            };

            if (!hasAccess)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            await next();
        }

        private async Task<bool> CheckArticleOwnerAsync(ActionExecutingContext context, string currentUserId)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idObj) ||
                !int.TryParse(idObj?.ToString(), out int articleId))
            {
                return false;
            }

            var articleService = context.HttpContext.RequestServices.GetService<IArticleService>();
            if (articleService == null) return false;

            try
            {
                var article = await articleService.GetArticleByIdAsync(articleId);
                return article != null && article.AuthorId.ToString() == currentUserId;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CheckCommentOwnerAsync(ActionExecutingContext context, string currentUserId)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idObj) ||
                !int.TryParse(idObj?.ToString(), out int commentId))
            {
                return false;
            }

            var commentService = context.HttpContext.RequestServices.GetService<ICommentService>();
            if (commentService == null) return false;

            try
            {
                var comment = await commentService.GetCommentByIdAsync(commentId);
                return comment != null && comment.AuthorId.ToString() == currentUserId;
            }
            catch
            {
                return false;
            }
        }

        private Task<bool> CheckUserOwnerAsync(ActionExecutingContext context, string currentUserId)
        {
            if (!context.ActionArguments.TryGetValue("id", out var idObj) ||
                !int.TryParse(idObj?.ToString(), out int userId))
            {
                return Task.FromResult(false);
            }

            bool isOwner = userId.ToString() == currentUserId;
            return Task.FromResult(isOwner);
        }
    }
}