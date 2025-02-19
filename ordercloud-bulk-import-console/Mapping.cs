using ordercloud_bulk_import_console.digikey.Models;
using ordercloud_bulk_import_library;

namespace ordercloud_bulk_import_console
{
    internal static class Mapping
    {
        internal static OcProduct MapTo(DigiKeyProduct row)
        {
            var facets = new DigiKeyFacets();

            if (row.Parameters != null)
            {
                foreach (var param in row.Parameters)
                {
                    if (param?.ParameterText != null &&
                        ParameterKeyMap.TryGetValue(param.ParameterText, out var key) &&
                        !string.IsNullOrWhiteSpace(param.ValueText))
                    {
                        typeof(DigiKeyFacets).GetProperty(key)?.SetValue(facets, param.ValueText);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(row.Category?.Name))
                facets.CategoryName = row.Category.Name;

            if (!string.IsNullOrWhiteSpace(row.Manufacturer?.Name))
                facets.ManufacturerName = row.Manufacturer.Name;

            if (!string.IsNullOrWhiteSpace(row.Series?.Name))
                facets.Series = row.Series.Name;

            if (row.ProductVariations != null)
            {
                var uniquePackageTypes = row.ProductVariations
                    .Select(v => v?.PackageType?.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct()
                    .ToList();

                if (uniquePackageTypes.Any())
                    facets.Packaging = uniquePackageTypes;
            }

            if (!string.IsNullOrWhiteSpace(row.ProductStatus?.Status))
                facets.PartStatus = row.ProductStatus.Status;

            return new OcProduct
            {
                ID = row.ManufacturerProductNumber.Slugify(),
                Name = row.Description?.ProductDescription,
                Active = true,
                Description = row.Description?.DetailedDescription,
                IsParent = true,
                xp = new OcProductXp
                {
                    DigiKeyPartNumber = row.ProductVariations?.FirstOrDefault()?.DigiKeyProductNumber,
                    Facets = facets,
                    Images = new List<ImageDefinition>
                    {
                        new ImageDefinition
                        {
                            ThumbnailUrl = row.PhotoUrl,
                            Url = row.PhotoUrl
                        }
                    }
                }
            };
        }

        internal static string MapToCategoryId(DigiKeyProduct row)
        {
            return row.Category.CategoryId.ToString();
        }

        private static readonly Dictionary<string, string> ParameterKeyMap = new()
        {
            ["Driver Circuitry"] = "DriverCircuitry",
            ["Input Type"] = "InputType",
            ["Voltage - Rated"] = "VoltageRated",
            ["Voltage Range"] = "VoltageRange",
            ["Frequency"] = "Frequency",
            ["Technology"] = "Technology",
            ["Operating Mode"] = "OperatingMode",
            ["Duration"] = "Duration",
            ["Sound Pressure Level (SPL)"] = "SoundPressureLevelSPL",
            ["Current - Supply"] = "CurrentSupply",
            ["Port Location"] = "PortLocation",
            ["Operating Temperature"] = "OperatingTemperature",
            ["Approval Agency"] = "ApprovalAgency",
            ["Ratings"] = "Ratings",
            ["Mounting Type"] = "MountingType",
            ["Termination"] = "Termination",
            ["Size / Dimension"] = "SizeDimension",
            ["Height - Seated (Max)"] = "HeightSeatedMax",
            ["Acccesory Type"] = "AccessoryType",
            ["For Use With/Related Products"] = "ForUseWithRelatedProducts",
            ["Battery Type, Function"] = "BatteryTypeFunction",
            ["Style"] = "Style",
            ["Battery Cell Size"] = "BatteryCellSize",
            ["Number of Cells"] = "NumberOfCells",
            ["Battery Series"] = "BatterySeries",
            ["Termination Style"] = "TerminationStyle",
            ["Height Above Board"] = "HeightAboveBoard",
            ["Battery Chemistry"] = "BatteryChemistry",
            ["Capacity"] = "Capacity",
            ["Features"] = "Features"
        };
    }
}
