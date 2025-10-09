using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Models
{
    public class Role
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название роли обязательно")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Описание роли обязательно")]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
