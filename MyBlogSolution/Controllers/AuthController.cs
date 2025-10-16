using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Services;
using MyBlog.Web.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ILoggerService _loggerService;

        public AuthController(IUserService userService, IRoleService roleService, ILoggerService loggerService)
        {
            _userService = userService;
            _roleService = roleService;
            _loggerService = loggerService;
        }

        // GET: Auth/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        private async Task SignInUserAsync(User user, bool rememberMe = true)
        {
            var userWithRoles = await _userService.GetUserWithRolesAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? "")
            };

            if (userWithRoles?.Roles != null)
            {
                foreach (var role in userWithRoles.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 7 : 1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // POST: Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isValid = await _userService.ValidateUserAsync(email, password);
                    if (isValid)
                    {
                        var user = await _userService.GetUserByEmailAsync(email);
                        var userWithRoles = await _userService.GetUserWithRolesAsync(user.Id);

                        await _loggerService.LogUserActionAsync("LOGIN",
                        $"User logged in successfully. Email: {email}",
                        user.Id, $"{user.FirstName} {user.LastName}");

                        // Создаем claims
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                            new Claim("FirstName", user.FirstName ?? ""),
                            new Claim("LastName", user.LastName ?? "")
                        };

                        // Добавляем роли в claims
                        if (userWithRoles?.Roles != null)
                        {
                            foreach (var role in userWithRoles.Roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role.Name));
                            }
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        await _loggerService.LogUserActionAsync("LOGIN_FAILED",
                        $"Failed login attempt. Email: {email}");

                        ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                    }
                }
                catch (Exception ex)
                {
                    await _loggerService.LogErrorAsync("Error during login", ex);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View();
        }

        // GET: Auth/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string firstName, string lastName)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userService.RegisterAsync(email, password, firstName, lastName);

                    await _loggerService.LogUserActionAsync("REGISTER",
                        $"User registered successfully. Email: {email}",
                        user.Id, $"{user.FirstName} {user.LastName}");

                    return await Login(email, password);
                }
                catch (Exception ex)
                {
                    await _loggerService.LogErrorAsync("Error during registration", ex);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View();
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            int? userId = null;

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            await _loggerService.LogUserActionAsync("LOGOUT",
                "User logged out",
                userId);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Auth/AccessDenied
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

        // GET: Auth/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                await _loggerService.LogErrorAsync("Error accessing profile - user not authenticated");
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserWithRolesAsync(userId);
            return View(user);
        }

        // GET: Auth/EditProfile
        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                await _loggerService.LogErrorAsync("Error accessing edit profile - user not authenticated");
                return RedirectToAction("Login");
            }

            var user = await _userService.GetUserByIdAsync(userId);

            var editModel = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return View(editModel);

        }

        // POST: Auth/EditProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    {
                        await _loggerService.LogErrorAsync("Error editing profile - user not authenticated");
                        return RedirectToAction("Login");
                    }

                    var user = await _userService.GetUserByIdAsync(userId);

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    await _userService.UpdateUserAsync(user);

                    TempData["Success"] = "Профиль успешно обновлен!";
                    return RedirectToAction(nameof(Profile));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(model);
        }
    }
}

