using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } // Например, "C#", "ASP.NET", "Programming"

        // Навигационное свойство для связи со статьями
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
