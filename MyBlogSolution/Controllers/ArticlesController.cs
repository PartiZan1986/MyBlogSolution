using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Web.Attributes;
using System.Security.Claims;

namespace MyBlog.Web.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IUserService _userService;
        private readonly ITagService _tagService;
        private readonly ILoggerService _loggerService;

        public ArticlesController(IArticleService articleService, IUserService userService, ITagService tagService, ILoggerService loggerService)
        {
            _articleService = articleService;
            _userService = userService;
            _tagService = tagService;
            _loggerService = loggerService;
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [CheckOwner("article")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    await _loggerService.LogErrorAsync("Error deleting article - user not authenticated");
                    return RedirectToAction("Login", "Auth");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                var article = await _articleService.GetArticleByIdAsync(id);

                await _articleService.DeleteArticleAsync(id);

                // Логируем удаление статьи
                await _loggerService.LogUserActionAsync("ARTICLE_DELETE",
                    $"Article deleted. ID: {id}, Title: {article.Title}",
                    userId, $"{user.FirstName} {user.LastName}");

                TempData["Success"] = "Статья успешно удалена!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                await _loggerService.LogErrorAsync($"Error deleting article - article with ID {id} not found", ex);
                return NotFound();
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error deleting article", ex);
                TempData["Error"] = "Произошла ошибка при удалении статьи";
                return RedirectToAction(nameof(Index));
            }
        }
        

        // GET: Articles
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return View(articles);
        }

        // GET: Articles/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                return View(article);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: Articles/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var allTags = await _tagService.GetAllTagsAsync();
            ViewBag.AllTags = allTags;
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string summary, string content, string tags)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        await _loggerService.LogErrorAsync("Error creating article - user not authenticated");
                        return RedirectToAction("Login", "Auth");
                    }

                    var user = await _userService.GetUserByIdAsync(userId);

                    // Преобразуем строку тегов в List<string>
                    var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrWhiteSpace(t))
                                      .ToList() ?? new List<string>();

                    var article = await _articleService.CreateArticleAsync(title, summary, content, userId, tagList);

                    await _loggerService.LogUserActionAsync("ARTICLE_CREATE",
                    $"Article created. ID: {article.Id}, Title: {title}",
                    userId, $"{user.FirstName} {user.LastName}");

                    TempData["Success"] = "Статья успешно создана!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _loggerService.LogErrorAsync("Error creating article", ex);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // Если есть ошибки, возвращаем представление с данными и списком тегов
            var allTags = await _tagService.GetAllTagsAsync();
            ViewBag.AllTags = allTags;
            return View();
        }

        // GET: Articles/Edit/5
        [Authorize]
        [CheckOwner("article")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                var allTags = await _tagService.GetAllTagsAsync();
                ViewBag.AllTags = allTags;
                return View(article);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [Authorize]
        [CheckOwner("article")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Article article)
        {
            if (id != article.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        await _loggerService.LogErrorAsync("Error editing article - user not authenticated");
                        return RedirectToAction("Login", "Auth");
                    }

                    var user = await _userService.GetUserByIdAsync(userId);

                    await _articleService.UpdateArticleAsync(article);

                    // Логируем редактирование статьи
                    await _loggerService.LogUserActionAsync("ARTICLE_EDIT",
                        $"Article edited. ID: {article.Id}, Title: {article.Title}",
                        userId, $"{user.FirstName} {user.LastName}");

                    TempData["Success"] = "Статья успешно обновлена!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await _loggerService.LogErrorAsync("Error editing article", ex);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // Если есть ошибки, загружаем теги снова
            var allTags = await _tagService.GetAllTagsAsync();
            ViewBag.AllTags = allTags;
            return View(article);
        }

        // GET: Articles/Delete/5
        [Authorize]
        [CheckOwner("article")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                return View(article);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }        
    }
}