using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Web.Attributes;
using System.Security.Claims;

namespace MyBlog.Web.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IArticleService _articleService;

        public CommentsController(ICommentService commentService, IArticleService articleService)
        {
            _commentService = commentService;
            _articleService = articleService;
        }

        // GET: Comments
        [AuthorizeRoles("Admin", "Moderator")] // Только админы и модераторы
        public async Task<IActionResult> Index()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return View(comments);
        }

        // POST: Comments/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string text, int articleId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var authorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                    await _commentService.CreateCommentAsync(text, articleId, authorId);
                    return RedirectToAction("Details", "Articles", new { id = articleId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return RedirectToAction("Details", "Articles", new { id = articleId });
        }

        // GET: Comments/Edit/5
        [Authorize]
        [CheckOwner("comment")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                return View(comment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [Authorize]
        [CheckOwner("comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Comment comment)
        {
            if (id != comment.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _commentService.UpdateCommentAsync(comment);
                    return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(comment);
        }

        // GET: Comments/Delete/5
        [Authorize]
        [CheckOwner("comment")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                return View(comment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [CheckOwner("comment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                await _commentService.DeleteCommentAsync(id);
                return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Comments/Delete/5 (для админов/модераторов)
        [HttpPost]
        [AuthorizeRoles("Admin", "Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteByModerator(int id)
        {
            try
            {
                var comment = await _commentService.GetCommentByIdAsync(id);
                await _commentService.DeleteCommentAsync(id);
                return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}