using System;

namespace AVBDelivery.Features.Orders
{
    public static class OrderConstants
    {
        public static class CacheKeys
        {
            public const string CartPrefix = "cart";
            public const string Cart = CartPrefix + ":{0}:{1:yyyy-MM-dd}";
            public const string UploadPreview = UploadPrefix + ":{0}:preview";
            public const string UploadPrefix = "upload";
        }

        public static class CacheExpiration
        {
            public static readonly TimeSpan CartTtl = TimeSpan.FromHours(2);
            public static readonly TimeSpan UploadPreviewTtl = TimeSpan.FromMinutes(10);
        }

        public static class AmoCrm
        {
            public const string CatalogTypeProducts = "products";
            public const string EntityTypeCatalogElements = "catalog_elements";
            public const string FieldCodePrice = "PRICE";

            public const int NewClientPipelineId = 9457550;

            public const long InvoiceLinkFieldId = 964597;

            public static class LeadFields
            {
                public const string DeliveryDate = "Дата доставки";
                public const string DeliveryTimeWeekday = "Время для буднего";
                public const string DeliveryTimeWeekend = "Время для выходного";
                public const string Notes = "Примечание";
            }
        }

        public static class Log
        {
            public const string LevelInfo = "INFO";
            public const string MessageOrderCreated = "Создан заказ";
            public const string MessageOrderCreatedViaUpload = "Создан заказ (загрузка xlsx, локация: {0})";
        }

        public static class Messages
        {
            public const string CartEmpty = "Корзина пуста — заказ не создан.";
            public const string MenuChanged = "Меню изменилось. Товары больше не доступны: {0}. Обновите корзину.";
            public const string OrderBelowMinimum = "Заказ не создан. Заказ меньше минимальной суммы. Сумма заказа: {0}. Минимальная сумма: {1}";
            public const string ContactMissing = "Заказ не создан. Отсутствует контакт. Обратитесь к системному администратору";
            public const string CompanyMissing = "Заказ не создан. Отсутствует компания. Обратитесь к системному администратору";
            public const string CatalogMissing = "Заказ не создан. Отсутствует каталог товаров. Обратитесь к системному администратору";
            public const string PriceFieldMissing = "Заказ не создан. Отсутствует поле цены. Обратитесь к системному администратору";
            public const string OrderCreateFailed = "Заказ не создан. Обратитесь к системному администратору";
            public const string OrderCreatedButLinkFailed = "Заказ №{0} создан, но товары не прикрепились в amoCRM. Обратитесь к администратору";
            public const string OrderCreated = "Заказ №{0} успешно создан.";

            public const string FileNotSelected = "Файл не выбран.";
            public const string FileNoSheets = "Файл не содержит листов.";
            public const string FileNoData = "Файл не содержит данных.";
            public const string NoAddressColumns = "Не найдены колонки с адресами (ожидались с 4-й колонки).";
            public const string UploadPreviewExpired = "Данные загрузки устели. Загрузите файл заново.";
            public const string UploadNoData = "Нет данных для создания заказов.";
            public const string UploadOrdersCreated = "Создано заказов: {0} (№{1})";
            public const string UploadOrdersFailed = "Заказы не созданы. Проверьте минимальную сумму и меню.";
        }

        public static class Xlsx
        {
            public const string TotalColumnRu = "Итого";
            public const string TotalColumnEn = "Total";
            public const int FirstDataColumn = 4;
            public const int FirstDataRow = 2;
        }

        public static class Roles
        {
            public const string Admin = "admin";
        }
    }
}
