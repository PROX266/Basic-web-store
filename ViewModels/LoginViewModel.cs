using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";
    }
}
