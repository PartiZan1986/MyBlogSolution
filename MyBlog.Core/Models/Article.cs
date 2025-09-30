using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyBlog.Core.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Содержание обязательно")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Внешний ключ на автора (User)
        public int AuthorId { get; set; }
        // Навигационное свойство на автора
        public virtual User Author { get; set; }

        // Связь многие-ко-многим с тегами
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        // Связь один-ко-многим с комментариями
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
