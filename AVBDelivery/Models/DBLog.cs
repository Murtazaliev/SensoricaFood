using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVBDelivery.Models
{
    public class DBLog
    {
        public int Id { get; set; }
        [Display(Name = "Время")]
        public DateTime DateTime { get; set; } = DateTime.Now;
        [Display(Name = "Важность")]
        public string? Level { get; set; }
        [Display(Name = "Пользователь")]
        public string? User { get; set; }
        [Display(Name = "Сообщение")]
        public string? Message { get; set; }
        [Display(Name = "Дополнительная информация")]
        public string? AdditionalInfo { get; set; }

    }
}
