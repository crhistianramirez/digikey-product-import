using OrderCloud.SDK;

namespace ordercloud_bulk_import_console
{
    public class OcProduct : Product<OcProductXp>;

    public class OcProductXp
    {
        public string DigiKeyPartNumber { get; set; }
        public List<ImageDefinition> Images { get; set; }
        public DigiKeyFacets Facets { get; set; }
    }

    public class ImageDefinition
    {
        public string ThumbnailUrl { get; set; }
        public string Url { get; set; }
    }

    public class DigiKeyFacets
    {
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string Series { get; set; }
        public List<string?>? Packaging { get; set; }
        public string PartStatus { get; set; }
        public string DriverCircuitry { get; set; }
        public string InputType { get; set; }
        public string VoltageRated { get; set; }
        public string VoltageRange { get; set; }
        public string Frequency { get; set; }
        public string Technology { get; set; }
        public string OperatingMode { get; set; }
        public string Duration { get; set; }
        public string SoundPressureLevelSPL { get; set; }
        public string CurrentSupply { get; set; }
        public string PortLocation { get; set; }
        public string OperatingTemperature { get; set; }
        public string ApprovalAgency { get; set; }
        public string Ratings { get; set; }
        public string MountingType { get; set; }
        public string Termination { get; set; }
        public string SizeDimension { get; set; }
        public string HeightSeatedMax { get; set; }
        public string AccessoryType { get; set; }
        public string ForUseWithRelatedProducts { get; set; }
        public string BatteryTypeFunction { get; set; }
        public string Style { get; set; }
        public string BatteryCellSize { get; set; }
        public int? NumberOfCells { get; set; }
        public string BatterySeries { get; set; }
        public string TerminationStyle { get; set; }
        public string HeightAboveBoard { get; set; }
        public string BatteryChemistry { get; set; }
        public string Capacity { get; set; }
        public string Features { get; set; }
    }
}
