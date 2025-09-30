using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Внешний ключ на статью
        public int ArticleId { get; set; }
        // Навигационное свойство на статью
        public virtual Article Article { get; set; }

        // Внешний ключ на автора (User)
        public int AuthorId { get; set; }
        // Навигационное свойство на автора
        public virtual User Author { get; set; }
    }
}
