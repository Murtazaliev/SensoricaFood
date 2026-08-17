using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVBDelivery.Models
{
    public class DateOfComing
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        [NotMapped]
        [Display(Name = "Время посещения")]
        public TimeOnly TimeOnly
        {
            get
            {
                return TimeOnly.FromDateTime(DateTime);
            }
            set
            {
                DateTime = new DateTime();
                DateTime += value.ToTimeSpan();
            }
        }
    }
}
