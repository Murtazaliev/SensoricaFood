using System;
using System.ComponentModel.DataAnnotations;

namespace AVBDelivery.ViewModels
{
    public class WorkingHoursViewModel
    {

        public int Id { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Начало работы")]
        public TimeOnly startTime { get; set; }
        [Display(Name = "Окончание работы")]
        public TimeOnly endTime { get; set; }
        public string ReturnUrl { get; set; }

    }
}
