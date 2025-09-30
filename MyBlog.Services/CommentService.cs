using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly IUserRepository _userRepository;

        public CommentService(ICommentRepository commentRepository, IArticleRepository articleRepository, IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _articleRepository = articleRepository;
            _userRepository = userRepository;
        }

        public async Task<Comment> CreateCommentAsync(string text, int articleId, int authorId)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Текст комментария не может быть пустым");

            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
                throw new ArgumentException("Статья не найдена");

            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
                throw new ArgumentException("Автор не найден");

            var comment = new Comment
            {
                Text = text.Trim(),
                ArticleId = articleId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            return comment;
        }

        public async Task<Comment> GetCommentByIdAsync(int id)
        {
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
        {
            return await _commentRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByArticleIdAsync(int articleId)
        {
            return await _commentRepository.GetByArticleIdAsync(articleId);
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            await _commentRepository.UpdateAsync(comment);
        }

        public async Task DeleteCommentAsync(int id)
        {
            await _commentRepository.DeleteAsync(id);
        }
    }
}
