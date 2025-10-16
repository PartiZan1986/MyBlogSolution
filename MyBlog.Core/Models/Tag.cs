using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Models
{
    public class Tag : BaseEntity
    {
        [Required(ErrorMessage = "Название тега обязательно")]
        [StringLength(50, ErrorMessage = "Название тега не должно превышать 50 символов")]
        public string Name { get; set; } = string.Empty;

        // Навигационное свойство для связи со статьями
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
