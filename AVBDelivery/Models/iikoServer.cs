using System.ComponentModel.DataAnnotations;

namespace AVBDelivery.Models
{
    public class IikoServer
    {
        public int Id { get; set; }

        [Display(Name = "Адрес сервера")]
        public string Server { get; set; }


        [Display(Name = "https")]
        public bool UseHttps { get; set; }


        [Display(Name = "Порт")]
        public int Port { get; set; }


        [Display(Name = "Путь")]
        public string Path { get; set; }


        [Display(Name = "Логин")]
        public string iikoLogin { get; set; }


        [Display(Name = "Пароль")]
        public string iikoPassword { get; set; }
    }
}


