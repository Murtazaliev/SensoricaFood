using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class OrganizationEditViewModel
    {
        public string? UserId { get; set; }
        public string OrganizationId { get; set; }
        public bool IsDeleted { get; set; } = false;

        [DisplayName("Название")]
        [Required(ErrorMessage = "Не введено название")]
        public string Name { get; set; }


        [DisplayName("ИНН")]
        [Required(ErrorMessage = "Не введен ИНН")]
        public string Inn { get; set; }


        [DisplayName("Адрес")]
        [Required(ErrorMessage = "Не введен адрес")]
        public string DeliveryAddress { get; set; }


        [DisplayName("Время доставки")]
        [Required(ErrorMessage = "Не введено время доставки")]
        public string DeliveryTime { get; set; }
        

        [DisplayName("Время доставки в выходные")]
        [Required(ErrorMessage = "Не введено время доставки в выходные")]
        public string DeliveryWeekendTime { get; set; }


        [DisplayName("Минимальная сумма доставки")]
        [Required(ErrorMessage = "Не введена минимальная сумма доставки")]
        public int? MinimalSum { get; set; }


        [DisplayName("Номер")]
        [Required(ErrorMessage = "Не введен номер телефона")]
        public string? PhoneNumber { get; set; }


        [DisplayName("Комментарий")]
        [Required(ErrorMessage = "Не введен комментарий")]
        public string? Comment { get; set; }

        private decimal? _discount = 0;

        [DisplayName("Скидка")]
        [Required(ErrorMessage = "Не введена скидка")]
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

        [DisplayName("Примечание")]
        public string? Note { get; set; }

        [DisplayName("Меню")]
        public int? MenuId { get; set; }

        public List<Menu> AllMenus { get; set; } = new List<Menu>();

        public List<Contact> AllContacts { get; set; }
        public List<Contact> Contacts { get; set; }
        public List<Note> AllNotes { get; set; }
        public List<Note> Notes { get; set; }

        public List<int> SelectedNoteIds { get; set; } = new List<int>();
        public OrganizationEditViewModel()
        {
            AllContacts = new List<Contact>();
            Contacts = new List<Contact>();
            AllNotes = new List<Note>();
            Notes = new List<Note>();
            AllMenus = new List<Menu>();
        }
        
    }
}
