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

        public ArticlesController(IArticleService articleService, IUserService userService, ITagService tagService)
        {
            _articleService = articleService;
            _userService = userService;
            _tagService = tagService;
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
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                    // Преобразуем строку тегов в List<string>
                    var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrWhiteSpace(t))
                                      .ToList() ?? new List<string>();

                    await _articleService.CreateArticleAsync(title, summary, content, userId, tagList);
                    TempData["Success"] = "Статья успешно создана!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
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
                    await _articleService.UpdateArticleAsync(article);
                    TempData["Success"] = "Статья успешно обновлена!"; // Добавил TempData
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
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

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [CheckOwner("article")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _articleService.DeleteArticleAsync(id);
                TempData["Success"] = "Статья успешно удалена!"; // Добавил TempData
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}