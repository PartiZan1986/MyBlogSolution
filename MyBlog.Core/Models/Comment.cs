using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Models
{
    public class Comment : BaseEntity
    {
        [Required(ErrorMessage = "Текст комментария обязателен")]
        public string Text { get; set; } = string.Empty;
        // Внешний ключ на статью
        public int ArticleId { get; set; }
        // Навигационное свойство на статью
        public virtual Article Article { get; set; } = null!;

        // Внешний ключ на автора (User)
        public int AuthorId { get; set; }
        // Навигационное свойство на автора
        public virtual User Author { get; set; } = null!;
    }
}
