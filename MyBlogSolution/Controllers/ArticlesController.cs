using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Services;
using MyBlog.Web.Attributes;


namespace MyBlog.Web.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IUserService _userService;

        public ArticlesController(IArticleService articleService, IUserService userService)
        {
            _articleService = articleService;
            _userService = userService;
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string content, string tags)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Временное решение - позже добавим аутентификацию
                    var authorId = 1; // Заглушка - первый пользователь

                    var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(t => t.Trim())
                                      .Where(t => !string.IsNullOrWhiteSpace(t))
                                      .ToList() ?? new List<string>();

                    await _articleService.CreateArticleAsync(title, content, authorId, tagList);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
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
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
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
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
