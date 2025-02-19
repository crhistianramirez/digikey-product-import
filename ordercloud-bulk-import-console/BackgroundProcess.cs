using Flurl.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OrderCloud.SDK;

namespace ordercloud_bulk_import_console
{
    public class BackgroundProcess : BackgroundService
    {
        private readonly IHostApplicationLifetime appLifetime;
        private readonly ExportPipeline _export;
        private readonly ImportPipeline _import;

        public BackgroundProcess(
            ExportPipeline export,
            ImportPipeline import,
            IHostApplicationLifetime appLifetime)
        {
            this.appLifetime = appLifetime;
            _export = export;
            _import = import;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background process starting...");
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                await _export.RunAsync();
                //await _import.RunAsync();
                Console.WriteLine("Background process completed successfully");
            }
            catch (OrderCloudException ex)
            {
                var flurlException = ex.InnerException as FlurlHttpException;

                var errorLog = new
                {
                    ex.Message,
                    ex.Errors,
                    RequestMessage = flurlException.Message,
                    flurlException.Call.RequestBody,
                    RequestHeaders = flurlException.Call.Request.Headers
                };

                Console.WriteLine(JsonConvert.SerializeObject(errorLog, Formatting.Indented));
            }
            catch (FlurlHttpException ex)
            {
                Console.WriteLine($"An error occurred during background process");
                var errorLog = new
                {
                    Message = "",
                    Errors = "",
                    RequestMessage = ex.Message,
                    ex.Call.RequestBody,
                    RequestHeaders = ex.Call.Request.Headers
                };

                Console.WriteLine(JsonConvert.SerializeObject(errorLog, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during background process: {ex.Message}");
            }
            finally
            {
                appLifetime.StopApplication();
            }
        }
    }
}
