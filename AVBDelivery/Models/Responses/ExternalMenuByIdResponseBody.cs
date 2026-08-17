using System.Text.Json.Serialization;

namespace AVBDelivery.Models.Responses
{

    public class ExternalMenuByIdResponseBody
    {
        [JsonIgnore]
        public ProductCategory[] ProductCategories { get; set; }
        [JsonIgnore]
        public CustomerTagGroup[] CustomerTagGroups { get; set; }
        [JsonIgnore]
        public int Revision { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ButtonImageUrl { get; set; }
        [JsonIgnore]
        public string[] Intervals { get; set; }
        public ItemCategory[] ItemCategories { get; set; }
        [JsonIgnore]
        public string[] ComboCategories { get; set; }
    }

    public class ProductCategory
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool IsDeleted { get; set; }
        public double Percentage { get; set; }
    }

    public class CustomerTagGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool SelectSeveralTags { get; set; }
        public CustomerTagItem[] Items { get; set; }
    }

    public class CustomerTagItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ItemCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ButtonImageUrl { get; set; }
        public string HeaderImageUrl { get; set; }
        public string IikoGroupId { get; set; }
        public CategoryItem[] Items { get; set; }
        public string ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        [JsonIgnore]
        public string[] Schedules { get; set; }
        public bool IsHidden { get; set; }
        public string[] Tags { get; set; }
        public string[] Labels { get; set; }
    }

    public class CategoryItem
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Allergens { get; set; }
        public string[] Tags { get; set; }
        public string[] Labels { get; set; }
        public ItemSize[] ItemSizes { get; set; }
        public string ItemId { get; set; }
        public string ModifierSchemaId { get; set; }
        public string TaxCategory { get; set; }
        public string ModifierSchemaName { get; set; }
        public string Type { get; set; }
        public bool CanBeDivided { get; set; }
        public bool CanSetOpenPrice { get; set; }
        public bool UseBalanceForSell { get; set; }
        public string MeasureUnit { get; set; }
        public string ProductCategoryId { get; set; }
        public string[] CustomerTagGroups { get; set; }
        public string PaymentSubject { get; set; }
        public string OuterEanCode { get; set; }
        public bool IsHidden { get; set; }
        public string OrderItemType { get; set; }
    }

    public class ItemSize
    {
        public string Sku { get; set; }
        public string SizeCode { get; set; }
        public string SizeName { get; set; }
        public bool IsDefault { get; set; }
        public float PortionWeightGrams { get; set; }
        public ItemModifierGroup[] ItemModifierGroups { get; set; }
        public string SizeId { get; set; }
        public NutritionPerHundredGrams NutritionPerHundredGrams { get; set; }
        public ItemPrice[] Prices { get; set; }
        public Nutrition[] Nutritions { get; set; }
        public bool IsHidden { get; set; }
        public string MeasureUnitType { get; set; }
        public string ButtonImageUrl { get; set; }
    }

    public class NutritionPerHundredGrams
    {
        public float Fats { get; set; }
        public float Proteins { get; set; }
        public float Carbs { get; set; }
        public float Energy { get; set; }
        public string[] Organizations { get; set; }
        public string SaturatedFattyAcid { get; set; }
        public string Salt { get; set; }
        public string Sugar { get; set; }
    }

    public class ItemModifierGroup
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Restrictions Restrictions { get; set; }
        public ModifierItem[] Items { get; set; }
        public bool CanBeDivided { get; set; }
        public string ItemGroupId { get; set; }
        public bool IsHidden { get; set; }
        public bool ChildModifiersHaveMinMaxRestrictions { get; set; }
        public string Sku { get; set; }
    }

    public class Restrictions
    {
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public int FreeQuantity { get; set; }
        public int ByDefault { get; set; }
        public bool HideIfDefaultQuantity { get; set; }
    }

    public class ModifierItem
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Restrictions Restrictions { get; set; }
        public string[] AllergenGroups { get; set; }
        public NutritionPerHundredGrams NutritionPerHundredGrams { get; set; }
        public int PortionWeightGrams { get; set; }
        public string[] Tags { get; set; }
        public string[] Labels { get; set; }
        public string ItemId { get; set; }
        public bool IsHidden { get; set; }
        public ItemPrice[] Prices { get; set; }
        public int Position { get; set; }
        public bool IndependentQuantity { get; set; }
        public string ProductCategoryId { get; set; }
        public string[] CustomerTagGroups { get; set; }
        public string PaymentSubject { get; set; }
        public string OuterEanCode { get; set; }
        public string MeasureUnitType { get; set; }
        public string ButtonImageUrl { get; set; }
    }

    public class ItemPrice
    {
        public string OrganizationId { get; set; }
        public double? Price { get; set; }
    }

    public class Nutrition
    {
        public float Fats { get; set; }
        public float Proteins { get; set; }
        public float Carbs { get; set; }
        public float Energy { get; set; }
        public string[] Organizations { get; set; }
        public string SaturatedFattyAcid { get; set; }
        public string Salt { get; set; }
        public string Sugar { get; set; }
    }

}
