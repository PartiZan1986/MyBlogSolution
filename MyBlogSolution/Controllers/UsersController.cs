using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Web.Attributes;
using System.Security.Claims;

namespace MyBlog.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UsersController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        // GET: Users
        [AuthorizeRoles("Admin")] // Только для администраторов
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        // GET: Users/Details/5
        [Authorize]
        [CheckOwner("user")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userService.GetUserWithRolesAsync(id);
                return View(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: Users/Edit/5
        [Authorize]
        [CheckOwner("user")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return View(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [Authorize]
        [CheckOwner("user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _userService.UpdateUserAsync(user);
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(user);
        }

        // GET: Users/Delete/5
        [AuthorizeRoles("Admin")] // Только администраторы могут удалять пользователей
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return View(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [AuthorizeRoles("Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: Users/ManageRoles/5
        [AuthorizeRoles("Admin")]
        public async Task<IActionResult> ManageRoles(int id)
        {
            try
            {
                var user = await _userService.GetUserWithRolesAsync(id);
                var allRoles = await _roleService.GetAllRolesAsync();

                ViewBag.AllRoles = allRoles;
                return View(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: Users/AssignRole
        [HttpPost]
        [AuthorizeRoles("Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            try
            {
                await _roleService.AssignRoleToUserAsync(userId, roleId);
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
        }

        // POST: Users/RemoveRole
        [HttpPost]
        [AuthorizeRoles("Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(int userId, int roleId)
        {
            try
            {
                await _roleService.RemoveRoleFromUserAsync(userId, roleId);
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
        }
    }
}