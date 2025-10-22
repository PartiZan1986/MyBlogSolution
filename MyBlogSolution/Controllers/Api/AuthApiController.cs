using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using System.Security.Claims;

namespace MyBlog.Web.Controllers.Api
{
    /// <summary>
    /// API для аутентификации и управления пользователями
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILoggerService _loggerService;

        public AuthApiController(IUserService userService, ILoggerService loggerService)
        {
            _userService = userService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Вход пользователя в систему
        /// </summary>
        /// <param name="request">Данные для входа</param>
        /// <returns>Информация о пользователе и результат входа</returns>
        /// <response code="200">Успешный вход</response>
        /// <response code="401">Неверные учетные данные</response>
        /// <response code="400">Ошибка валидации</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var isValid = await _userService.ValidateUserAsync(request.Email, request.Password);
                if (!isValid)
                {
                    await _loggerService.LogUserActionAsync("LOGIN_FAILED_API",
                        $"Failed login attempt via API. Email: {request.Email}");

                    return Unauthorized("Invalid email or password");
                }

                var user = await _userService.GetUserByEmailAsync(request.Email);

                await _loggerService.LogUserActionAsync("LOGIN_API",
                    $"User logged in via API. Email: {request.Email}",
                    user.Id, $"{user.FirstName} {user.LastName}");

                return Ok(new LoginResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error during API login", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="request">Данные для регистрации</param>
        /// <returns>Информация о зарегистрированном пользователе</returns>
        /// <response code="200">Успешная регистрация</response>
        /// <response code="400">Ошибка валидации или пользователь уже существует</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _userService.RegisterAsync(
                    request.Email,
                    request.Password,
                    request.FirstName,
                    request.LastName
                );

                await _loggerService.LogUserActionAsync("REGISTER_API",
                    $"User registered via API. Email: {request.Email}",
                    user.Id, $"{user.FirstName} {user.LastName}");

                return Ok(new RegisterResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error during API registration", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получить профиль текущего пользователя
        /// </summary>
        /// <returns>Профиль пользователя с ролями и статистикой</returns>
        /// <response code="200">Возвращает профиль пользователя</response>
        /// <response code="401">Не авторизован</response>
        /// <response code="404">Пользователь не найден</response>
        /// <response code="500">Ошибка сервера</response>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserProfileResponse>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var user = await _userService.GetUserWithRolesAsync(userId);

                if (user == null)
                {
                    await _loggerService.LogErrorAsync("User not found when accessing profile via API", null, userId);
                    return NotFound("User not found");
                }

                return Ok(new UserProfileResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Roles = user.Roles?.Select(r => r.Name).ToList() ?? new List<string>(),
                    ArticlesCount = user.Articles?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error getting user profile via API", ex);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    /// <summary>
    /// Запрос для входа в систему
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        /// <example>user@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        /// <example>password123</example>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ответ после успешного входа
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        /// <example>1</example>
        public int UserId { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        /// <example>user@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        /// <example>John</example>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        /// <example>Doe</example>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Сообщение о результате операции
        /// </summary>
        /// <example>Login successful</example>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Запрос для регистрации пользователя
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        /// <example>newuser@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        /// <example>password123</example>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        /// <example>John</example>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        /// <example>Doe</example>
        public string LastName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ответ после успешной регистрации
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        /// <example>1</example>
        public int UserId { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        /// <example>newuser@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        /// <example>John</example>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        /// <example>Doe</example>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Сообщение о результате операции
        /// </summary>
        /// <example>Registration successful</example>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Профиль пользователя
    /// </summary>
    public class UserProfileResponse
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        /// <example>user@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        /// <example>John</example>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        /// <example>Doe</example>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Список ролей пользователя
        /// </summary>
        /// <example>["User", "Moderator"]</example>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Количество статей пользователя
        /// </summary>
        /// <example>5</example>
        public int ArticlesCount { get; set; }
    }
}