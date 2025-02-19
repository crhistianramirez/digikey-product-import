#nullable enable

namespace ordercloud_bulk_import_console.digikey.Models
{
    public class DigiKeySearchResponseBody
    {
        public List<DigiKeyProduct>? Products { get; set; }
        public int? ProductsCount { get; set; }
    }

    public class DigiKeyProduct
    {
        public Description? Description { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public string? ManufacturerProductNumber { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? ProductUrl { get; set; }
        public string? DatasheetUrl { get; set; }
        public string? PhotoUrl { get; set; }
        public List<ProductVariation>? ProductVariations { get; set; }
        public int? QuantityAvailable { get; set; }
        public ProductStatus? ProductStatus { get; set; }
        public bool? BackOrderNotAllowed { get; set; }
        public bool? NormallyStocking { get; set; }
        public bool? Discontinued { get; set; }
        public bool? EndOfLife { get; set; }
        public bool? Ncnr { get; set; }
        public string? PrimaryVideoUrl { get; set; }
        public List<Parameter>? Parameters { get; set; }
        public object? BaseProductNumber { get; set; }
        public Category? Category { get; set; }
        public object? DateLastBuyChance { get; set; }
        public string? ManufacturerLeadWeeks { get; set; }
        public int? ManufacturerPublicQuantity { get; set; }
        public Series? Series { get; set; }
        public object? ShippingInfo { get; set; }
        public Classifications? Classifications { get; set; }
        public List<string>? OtherNames { get; set; }
    }

    public class Description
    {
        public string? ProductDescription { get; set; }
        public string? DetailedDescription { get; set; }
    }

    public class Manufacturer
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }

    public class ProductVariation
    {
        public string? DigiKeyProductNumber { get; set; }
        public PackageType? PackageType { get; set; }
        public List<StandardPricing>? StandardPricing { get; set; }
        public List<object>? MyPricing { get; set; }
        public bool? MarketPlace { get; set; }
        public bool? TariffActive { get; set; }
        public Supplier? Supplier { get; set; }
        public int? QuantityAvailableforPackageType { get; set; }
        public int? MaxQuantityForDistribution { get; set; }
        public int? MinimumOrderQuantity { get; set; }
        public int? StandardPackage { get; set; }
        public decimal? DigiReelFee { get; set; }
    }

    public class PackageType
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }

    public class StandardPricing
    {
        public int? BreakQuantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class Supplier
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }

    public class ProductStatus
    {
        public int? Id { get; set; }
        public string? Status { get; set; }
    }

    public class Parameter
    {
        public int? ParameterId { get; set; }
        public string? ParameterText { get; set; }
        public string? ParameterType { get; set; }
        public string? ValueId { get; set; }
        public string? ValueText { get; set; }
    }

    public class Category
    {
        public int? CategoryId { get; set; }
        public int? ParentId { get; set; }
        public string? Name { get; set; }
        public int? ProductCount { get; set; }
        public int? NewProductCount { get; set; }
        public string? ImageUrl { get; set; }
        public string? SeoDescription { get; set; }
        public List<ChildCategory>? ChildCategories { get; set; }
    }

    public class ChildCategory
    {
        public int? CategoryId { get; set; }
        public int? ParentId { get; set; }
        public string? Name { get; set; }
        public int? ProductCount { get; set; }
        public int? NewProductCount { get; set; }
        public string? ImageUrl { get; set; }
        public string? SeoDescription { get; set; }
        public List<object>? ChildCategories { get; set; }
    }

    public class Series
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
    }

    public class Classifications
    {
        public string? ReachStatus { get; set; }
        public string? RohsStatus { get; set; }
        public string? MoistureSensitivityLevel { get; set; }
        public string? ExportControlClassNumber { get; set; }
        public string? HtsusCode { get; set; }
    }
}
