using System.Text.Json.Serialization;

namespace AVBDelivery.Models.AmoCrm
{
    public class CustomField
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("sort")]
        public int Sort { get; set; }
        [JsonPropertyName("is_api_only")]
        public bool IsApiOnly { get; set; }
        [JsonPropertyName("enums")]
        public Enum[] Enums { get; set; }
        [JsonPropertyName("catalog_id")]
        public int CatalogId { get; set; }
        [JsonPropertyName("is_visible")]
        public bool IsVisible { get; set; }
        [JsonPropertyName("triggers")]
        public string[]? Triggers { get; set; }
        [JsonPropertyName("is_deletable")]
        public bool IsDeletable { get; set; }
        [JsonPropertyName("is_required")]
        public bool IsRequired { get; set; }
        [JsonPropertyName("search_in")]
        public string? SearchIn { get; set; }
        [JsonPropertyName("nested")]
        public Nested[] Nested { get; set; }
        [JsonPropertyName("entity_type")]
        public string EntityType { get; set; }
        [JsonPropertyName("_links")]
        public Links Links { get; set; }
    }

    public class Enum
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("sort")]
        public int Sort { get; set; }
    }

    public class Nested
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("parent_id")]
        public int? ParentId { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }
        [JsonPropertyName("Sort")]
        public int Sort { get; set; }
    }
}
