using System.ComponentModel.DataAnnotations;

namespace AVBDelivery.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }
        [Display(Name = "Название")]
        public string Name { get; set; }
        [Display(Name = "Шаблон")]
        public string Template { get; set; }
    }
}
