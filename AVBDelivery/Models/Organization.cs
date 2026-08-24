using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AVBDelivery.Models
{
    public class Organization
    {
        public string OrganizationId { get; set; }
        public List<Contact>? Contacts { get; set; } = new List<Contact>();
        public bool IsDeleted { get; set; } = false;
        [DisplayName("Название")]
        public string Name { get; set; }
        [DisplayName("ИНН")]
        public string Inn { get; set; }
        [DisplayName("Адрес")]
        public string DeliveryAddress { get; set; }
        [DisplayName("Время доставки")]
        public string DeliveryTime { get; set; }
        [DisplayName("Время доставки в выходные")]
        public string DeliveryWeekendTime { get; set; }
        public string? AmoCrmId { get; set; }
        [DisplayName("Минимальная сумма доставки")]
        public int? MinimalSum { get; set; }
        [DisplayName("Номер")]
        public string? PhoneNumber { get; set; }
        [DisplayName("Комментарий")]
        public string? Comment { get; set; }

        private decimal? _discount = 0;

        [DisplayName("Скидка")]
        public decimal? Discount
        {
            get => _discount;
            set
            {
                if (value == null)
                {
                    _discount = null;
                }
                else if (value < 0)
                {
                    _discount = 0;
                }
                else if (value > 100)
                {
                    _discount = 100;
                }
                else
                {
                    _discount = value;
                }
            }
        }

        public ICollection<Note> Notes { get; set; } = new List<Note>();

        [DisplayName("Меню")]
        public int? MenuId { get; set; }
        public Menu? Menu { get; set; }

        public decimal CalculateDiscount()
        {
            decimal discountValue = 1.0m - ((Discount ?? 0) / 100);
            return Math.Max(0, Math.Min(1, discountValue));
            }
        }
}
