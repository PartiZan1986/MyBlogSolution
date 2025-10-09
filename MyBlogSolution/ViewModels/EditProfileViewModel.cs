using System.ComponentModel.DataAnnotations;

namespace MyBlog.Web.ViewModels
{
    public class EditProfileViewModel
    {
        [Display(Name = "Имя")]
        [StringLength(50, ErrorMessage = "Имя не должно превышать 50 символов")]
        public string? FirstName { get; set; }

        [Display(Name = "Фамилия")]
        [StringLength(50, ErrorMessage = "Фамилия не должна превышать 50 символов")]
        public string? LastName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string? Email { get; set; }
    }
}
