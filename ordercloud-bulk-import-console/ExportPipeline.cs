using ordercloud_bulk_import_console.digikey;
using ordercloud_bulk_import_console.digikey.Models;
using ordercloud_bulk_import_library;

namespace ordercloud_bulk_import_console
{
    // This pipeline is responsible for exporting data OUT OF Digikey into a JSON format
    public class ExportPipeline
    {
        private readonly IDigiKeyClient _digiKeyClient;

        public ExportPipeline(IDigiKeyClient digiKeyClient)
        {
            _digiKeyClient = digiKeyClient;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Starting export pipeline...");
            var products = new List<DigiKeyProduct>();
            var categoryIdList = new List<string> { "10", "6" }; // Audio, Battery
            const int limit = 50;
            const int totalPages = 5;

            foreach (var categoryId in categoryIdList)
            {
                Console.WriteLine($"Processing category {categoryId}...");
                for (int page = 0; page < totalPages; page++)
                {
                    int offset = page * limit;
                    Console.WriteLine($"Fetching page {page + 1} with offset {offset}...");

                    var response = await _digiKeyClient.SearchProducts(categoryId, limit: limit, offset: offset);
                    if (response == null || !response.Any())
                    {
                        Console.WriteLine("No more products found. Breaking loop.");
                        break;
                    }

                    Console.WriteLine($"Fetched {response.Count()} products.");
                    products.AddRange(response);
                    await Task.Delay(2000);
                }
            }

            Console.WriteLine("Removing duplicate products...");
            var uniqueProducts = products.DistinctBy(p => p.Description?.ProductDescription);

            var writeFilePath = FileOperations.GetDataFile($"digikey-products.json");
            Console.WriteLine($"Writing {uniqueProducts.Count()} unique products to {writeFilePath}...");
            FileOperations.FileCreateTextObject(writeFilePath, uniqueProducts);
            Console.WriteLine("Export pipeline completed successfully.");
        }
    }
}
