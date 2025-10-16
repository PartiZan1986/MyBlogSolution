using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyBlog.Core.Models
{
    public class Article : BaseEntity
    {
        [Required(ErrorMessage = "Заголовок обязателен")]
        [StringLength(200, ErrorMessage = "Заголовок не должен превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Краткое содержание обязательно")]
        [StringLength(500, ErrorMessage = "Краткое содержание не должно превышать 500 символов")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Содержание обязательно")]
        public string Content { get; set; } = string.Empty;
        
        // Внешний ключ на автора (User)
        public int AuthorId { get; set; }
        // Навигационное свойство на автора
        public virtual User Author { get; set; } = null!;

        // Связь многие-ко-многим с тегами
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
        // Связь один-ко-многим с комментариями
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
