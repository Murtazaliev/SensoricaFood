using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AVBDelivery.Models;

namespace AVBDelivery.ViewModels
{
    public class MenuViewModel
    {
        public int? Id { get; set; }

        [DisplayName("Название")]
        [Required(ErrorMessage = "Не введено название меню")]
        public string Name { get; set; }

        [DisplayName("Активно")]
        public bool IsActive { get; set; } = true;

        public List<MenuProductGroupViewModel> Groups { get; set; } = new List<MenuProductGroupViewModel>();
    }

    public class MenuProductGroupViewModel
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public List<MenuProductItemViewModel> Products { get; set; } = new List<MenuProductItemViewModel>();
    }

    public class MenuProductItemViewModel
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public float Price { get; set; }
        public bool IsSelected { get; set; }
    }
}