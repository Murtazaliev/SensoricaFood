namespace AVBDelivery.Models.Requests
{
    public class ExternalMenuByIdRequestBody
    {
        public string ExternalMenuId { get; set; }
        public string? PriceCategoryId { get; set; }
        public string[] OrganizationIds { get; set; }
    }
}
