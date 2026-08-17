using System.ComponentModel.DataAnnotations;

namespace AVBDelivery.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Не указан пароль")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Не указан номер телефона")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        public string? Name { get; set; }
        public bool IsClient { get; set; }
    }
}
