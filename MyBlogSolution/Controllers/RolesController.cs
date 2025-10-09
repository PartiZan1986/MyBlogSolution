using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Web.Attributes;
using System.Threading.Tasks;

namespace MyBlog.Web.Controllers
{
    [AuthorizeRoles("Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return View(roles);
        }

        // GET: Roles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                return View(role);
            }
            catch (System.Exception)
            {
                return NotFound();
            }
        }

        // GET: Roles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string description)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _roleService.CreateRoleAsync(name, description);
                    TempData["Success"] = "Роль успешно создана!";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View();
        }

        // GET: Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                return View(role);
            }
            catch (System.Exception)
            {
                return NotFound();
            }
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _roleService.UpdateRoleAsync(role);
                    TempData["Success"] = "Роль успешно обновлена!";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(id);
                return View(role);
            }
            catch (System.Exception)
            {
                return NotFound();
            }
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _roleService.DeleteRoleAsync(id);
                TempData["Success"] = "Роль успешно удалена!";
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception)
            {
                return NotFound();
            }
        }
    }
}
