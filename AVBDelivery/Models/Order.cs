using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AVBDelivery.Models
{
    public class Order // : IValidatableObject
    {
        public int Id { get; set; }
        /// <summary>
        /// Привязка к пользователю
        /// </summary>
        public string? UserId { get; set; }
        public string? OrganizationId { get; set; }

        public int? AmoCrmId { get; set; }

        /// <summary>
        /// Дата заказа
        /// </summary>
        [Display(Name = "Дата заказа")]
        public DateTime OrderDate { get; set; }
        /// <summary>
        /// Список позиций в заказе
        /// </summary>
        public List<OrderItem>? Items { get; set; }

        /// <summary>
        /// Сумма заказа
        /// </summary>
        public double? Sum { get; set; }

        /// <summary>
        /// Сумма заказа со скидкой
        /// </summary>
        public double? SumWithDiscount { get; set; }

        /// <summary>
        /// Время доставки
        /// </summary>
        public DateTime? DeliveryDate { get; set; }

        /// <summary>
        /// Ошибки валидации
        /// </summary>
        [NotMapped]
        public List<string>? ValidateErrors { get; set; }

    }

    public class OrderItem
    {
        public int Id { get; set; }
        /// <summary>
        /// Id продукта
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Название продукта
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Количество порций
        /// </summary>
        public double Count { get; set; }
        /// <summary>
        /// Цена за 1 порцию
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Единица измерения
        /// </summary>
        public string MeasureUnit { get; set; }
        public int? AmoCrmId { get; set; }
    }

}
