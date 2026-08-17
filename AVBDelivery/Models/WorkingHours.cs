using System;
using System.ComponentModel.DataAnnotations;

namespace AVBDelivery.Models
{
    [Display(Name = "Часы приёма заказов")]
    public class WorkingHours
    {
        public int Id { get; set; }
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Не указано имя")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string? Description { get; set; }
        [Display(Name = "Начало работы")]
        public DateTime startTime { get; set; }
        [Display(Name = "Окончание работы")]
        public DateTime endTime { get; set; }



    }
}
