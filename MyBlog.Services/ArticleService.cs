using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;

        public ArticleService(IArticleRepository articleRepository, ITagRepository tagRepository, IUserRepository userRepository)
        {
            _articleRepository = articleRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }

        public async Task<Article> CreateArticleAsync(string title, string content, int authorId, List<string> tagNames)
        {
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author == null)
                throw new ArgumentException("Автор не найден");

            var article = new Article
            {
                Title = title,
                Content = content,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            // Обрабатываем теги
            foreach (var tagName in tagNames)
            {
                var tag = await _tagRepository.GetByNameAsync(tagName);
                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    await _tagRepository.AddAsync(tag);
                }
                article.Tags.Add(tag);
            }

            await _articleRepository.AddAsync(article);
            return article;
        }

        public async Task<Article> GetArticleByIdAsync(int id)
        {
            return await _articleRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _articleRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Article>> GetArticlesByAuthorIdAsync(int authorId)
        {
            return await _articleRepository.GetByAuthorIdAsync(authorId);
        }

        public async Task<IEnumerable<Article>> GetArticlesByTagAsync(string tagName)
        {
            return await _articleRepository.GetByTagAsync(tagName);
        }

        public async Task UpdateArticleAsync(Article article)
        {
            article.UpdatedAt = DateTime.UtcNow;
            await _articleRepository.UpdateAsync(article);
        }

        public async Task DeleteArticleAsync(int id)
        {
            await _articleRepository.DeleteAsync(id);
        }

        public async Task AddCommentToArticleAsync(int articleId, string text, int authorId)
        {
            var article = await _articleRepository.GetByIdAsync(articleId);
            if (article == null)
                throw new ArgumentException("Статья не найдена");

            var comment = new Comment
            {
                Text = text,
                ArticleId = articleId,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow
            };

            article.Comments.Add(comment);
            await _articleRepository.UpdateAsync(article);
        }
    }
}
