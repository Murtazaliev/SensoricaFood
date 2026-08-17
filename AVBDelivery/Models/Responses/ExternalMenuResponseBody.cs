namespace AVBDelivery.Models.Responses
{
    public class ExternalMenusResponseBody
    {
        public string CorrelationId { get; set; }
        public ExternalMenu[] ExternalMenus { get; set; }
        public PriceCategory[] PriceCategories { get; set; }
    }

    public class ExternalMenu
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class PriceCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

}
