using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Interfaces
{
    public interface IArticleService
    {
        Task<Article> CreateArticleAsync(string title, string content, int authorId, List<string> tags);
        Task<Article> GetArticleByIdAsync(int id);
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<IEnumerable<Article>> GetArticlesByAuthorIdAsync(int authorId);
        Task<IEnumerable<Article>> GetArticlesByTagAsync(string tagName);
        Task UpdateArticleAsync(Article article);
        Task DeleteArticleAsync(int id);
        Task AddCommentToArticleAsync(int articleId, string text, int authorId);
    }
}
