using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Models;
using MyBlog.Core.Interfaces;

namespace MyBlog.Web.Controllers
{
    public class TagsController : Controller
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        // GET: Tags
        public async Task<IActionResult> Index()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return View(tags);
        }

        // GET: Tags/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                return View(tag);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: Tags/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tags/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _tagService.CreateTagAsync(name);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View();
        }

        // GET: Tags/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                return View(tag);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Tags/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _tagService.UpdateTagAsync(tag);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(tag);
        }

        // GET: Tags/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                return View(tag);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _tagService.DeleteTagAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
