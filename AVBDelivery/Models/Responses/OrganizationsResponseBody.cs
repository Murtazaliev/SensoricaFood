namespace AVBDelivery.Models.Responses
{
    public class OrganizationsResponseBody
    {
        public string CorrelationId { get; set; }
        public Organization[] Organizations { get; set; }
    }

    public class Organization
    {
        public string ResponseType { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }

}
