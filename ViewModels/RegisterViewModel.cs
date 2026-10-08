using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите логин")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
        [Display(Name = "Логин")]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100, ErrorMessage = "Имя не длиннее 100 символов")]
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не короче 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Повторите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Повтор пароля")]
        public string ConfirmPassword { get; set; } = "";
    }
}
