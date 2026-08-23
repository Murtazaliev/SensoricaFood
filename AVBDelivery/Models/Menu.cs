using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AVBDelivery.Models
{
    public class Menu
    {
        public int Id { get; set; }

        [DisplayName("Название")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Активно")]
        public bool IsActive { get; set; } = true;

        public List<MenuProduct> MenuProducts { get; set; } = new List<MenuProduct>();
    }

    // Связка меню с товарами (many-to-many).
    // ProductId хранится свободной строкой БЕЗ FK на Products:
    // NomenclatureUploader пересоздаёт все продукты при каждой синхронизации,
    // поэтому жёсткая связь удалила бы/сломала бы состав меню.
    public class MenuProduct
    {
        public int MenuId { get; set; }
        public string ProductId { get; set; }

        public Menu Menu { get; set; }
    }
}