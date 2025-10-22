using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using System.Security.Claims;

namespace MyBlog.Web.Controllers.Api
{
    /// <summary>
    /// API для управления статьями
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesApiController : ControllerBase
    {
        private readonly IArticleService _articleService;
        private readonly ILoggerService _loggerService;

        public ArticlesApiController(IArticleService articleService, ILoggerService loggerService)
        {
            _articleService = articleService;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Получить все статьи
        /// </summary>
        /// <returns>Список статей</returns>
        /// <response code="200">Возвращает список статей</response>
        /// <response code="500">Ошибка сервера</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Article>>> GetArticles()
        {
            try
            {
                var articles = await _articleService.GetAllArticlesAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error getting articles via API", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Получить статью по ID
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <returns>Статья</returns>
        /// <response code="200">Возвращает статью</response>
        /// <response code="404">Статья не найдена</response>
        /// <response code="500">Ошибка сервера</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Article>> GetArticle(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                return Ok(article);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync($"Error getting article {id} via API", ex);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Создать новую статью
        /// </summary>
        /// <param name="request">Данные для создания статьи</param>
        /// <returns>Созданная статья</returns>
        /// <response code="201">Статья успешно создана</response>
        /// <response code="400">Неверные данные</response>
        /// <response code="401">Не авторизован</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Article>> CreateArticle([FromBody] CreateArticleRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var article = await _articleService.CreateArticleAsync(
                    request.Title,
                    request.Summary,
                    request.Content,
                    userId,
                    request.Tags
                );

                await _loggerService.LogUserActionAsync("ARTICLE_CREATE_API",
                    $"Article created via API. ID: {article.Id}, Title: {request.Title}",
                    userId);

                return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync("Error creating article via API", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Обновить статью
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <param name="article">Данные статьи для обновления</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Статья успешно обновлена</response>
        /// <response code="400">Неверные данные</response>
        /// <response code="401">Не авторизован</response>
        /// <response code="404">Статья не найдена</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] Article article)
        {
            try
            {
                if (id != article.Id)
                    return BadRequest("ID mismatch");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                // TODO: Add authorization check - user can only update own articles

                await _articleService.UpdateArticleAsync(article);

                await _loggerService.LogUserActionAsync("ARTICLE_UPDATE_API",
                    $"Article updated via API. ID: {article.Id}, Title: {article.Title}",
                    userId);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync($"Error updating article {id} via API", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Удалить статью
        /// </summary>
        /// <param name="id">ID статьи</param>
        /// <returns>Результат операции</returns>
        /// <response code="204">Статья успешно удалена</response>
        /// <response code="401">Не авторизован</response>
        /// <response code="404">Статья не найдена</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                // TODO: Add authorization check - user can only delete own articles

                await _articleService.DeleteArticleAsync(id);

                await _loggerService.LogUserActionAsync("ARTICLE_DELETE_API",
                    $"Article deleted via API. ID: {id}",
                    userId);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                await _loggerService.LogErrorAsync($"Error deleting article {id} via API", ex);
                return BadRequest(ex.Message);
            }
        }
    }

    /// <summary>
    /// Запрос на создание статьи
    /// </summary>
    public class CreateArticleRequest
    {
        /// <summary>
        /// Заголовок статьи
        /// </summary>
        /// <example>Моя первая статья</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Краткое описание
        /// </summary>
        /// <example>Это краткое описание моей статьи</example>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Содержание статьи
        /// </summary>
        /// <example>Это полное содержание моей статьи...</example>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Список тегов
        /// </summary>
        /// <example>["csharp", "aspnet", "webapi"]</example>
        public List<string> Tags { get; set; } = new List<string>();
    }
}