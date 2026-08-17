using System;

namespace AVBDelivery.Models
{
    /// <summary>
    /// ShoppingCartItem, а не сама корзина
    /// </summary>
    public class ShoppingCart
    {
        public int Id { get; set; }
        /// <summary>
        /// Привязка к пользователю
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// Id продукта
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Название продукта
        /// </summary>
        public string? ProductName { get; set; }
        /// <summary>
        /// Количество порций
        /// </summary>
        public double Count { get; set; }
        /// <summary>
        /// Цена за 1 порцию
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Дата заказа
        /// </summary>
        public DateTime OrderDate { get; set; }
        public string? MeasureUnit { get; set; }
        public int? AmoCrmId { get; set; }
    }
}
