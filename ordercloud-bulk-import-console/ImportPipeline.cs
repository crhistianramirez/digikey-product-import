using OrderCloud.SDK;
using ordercloud_bulk_import_console.digikey.Models;
using ordercloud_bulk_import_library;

namespace ordercloud_bulk_import_console
{
    public class ImportPipeline
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _appSettings;
        public ImportPipeline(IOrderCloudClient oc, AppSettings appSettings)
        {
            _oc = oc;
            _appSettings = appSettings;
        }

        public async Task RunAsync()
        {
            // kick off tracking, status, error logging action items including reading in source file
            var (items, status, tracker) = Commands.StartJsonOperation<DigiKeyProduct>("digikey-products.json");

            await Commands.BatchOperation(1000, tracker, status, items, ProcessBatch);

            Commands.EndOperation(status, tracker);

            await tracker.CompleteAsync();
        }

        public async Task ProcessBatch(DigiKeyProduct item)
        {
            var product = Mapping.MapTo(item);
            await _oc.Products.SaveAsync(product.ID, product);
            await _oc.Catalogs.SaveProductAssignmentAsync(new ProductCatalogAssignment
            {
                CatalogID = _appSettings.OrderCloud.CatalogId,
                ProductID = product.ID
            });
            await _oc.Categories.SaveProductAssignmentAsync(_appSettings.OrderCloud.CatalogId, new CategoryProductAssignment
            {
                CategoryID = Mapping.MapToCategoryId(item),
                ProductID = product.ID
            });

            foreach (var variation in item.ProductVariations)
            {
                var childProductId = variation.DigiKeyProductNumber.Slugify();
                var priceBreaks = new List<PriceBreak> { };
                if (variation.StandardPricing != null && variation.StandardPricing.Count > 0)
                {
                    priceBreaks = variation.StandardPricing.Select(p =>
                        new PriceBreak
                        {
                            Price = p.UnitPrice ?? 0,
                            Quantity = p.BreakQuantity ?? 1
                        }
                    ).ToList();
                }
                await _oc.PriceSchedules.SaveAsync(childProductId, new PriceSchedule
                {
                    ID = childProductId,
                    Name = variation.PackageType?.Name,
                    PriceBreaks = priceBreaks
                });
                await _oc.Products.SaveAsync(childProductId, new Product
                {
                    Active = true,
                    ID = childProductId,
                    Name = variation.PackageType?.Name,
                    Description = item.Description?.DetailedDescription,
                    IsParent = false,
                    ParentID = product.ID
                });
            }
        }
    }
}
