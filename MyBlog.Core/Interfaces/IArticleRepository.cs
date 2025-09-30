using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Interfaces
{
    public interface IArticleRepository
    {
        Task<Article> GetByIdAsync(int id);
        Task<IEnumerable<Article>> GetAllAsync();
        Task<IEnumerable<Article>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<Article>> GetByTagAsync(string tagName);
        Task AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task DeleteAsync(int id);
        //Task AddCommentToArticleAsync(int articleId, string text, int authorId);
    }
}
