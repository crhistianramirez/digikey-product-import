using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderCloud.SDK;
using ordercloud_bulk_import_console;
using ordercloud_bulk_import_console.digikey;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => config.AddJsonFile("appsettings.json", optional: false))
    .ConfigureServices((context, services) =>
    {
        // read and bind app settings
        var appSettings = new AppSettings();
        context.Configuration.Bind(appSettings);
        services.Configure<AppSettings>(context.Configuration);
        services.Configure<DigiKeyClientOptions>(context.Configuration.GetSection("DigiKey"));

        // configure services
        services
            .AddSingleton(appSettings)
            .AddSingleton<IOrderCloudClient>(provider => new OrderCloudClient(new OrderCloudClientConfig
            {
                ApiUrl = appSettings.OrderCloud?.ApiUrl ?? throw new ArgumentNullException(nameof(appSettings.OrderCloud.ApiUrl)),
                AuthUrl = appSettings.OrderCloud?.ApiUrl ?? throw new ArgumentNullException(nameof(appSettings.OrderCloud.ApiUrl)),
                ClientId = appSettings.OrderCloud?.MiddlewareClientID ?? throw new ArgumentNullException(nameof(appSettings.OrderCloud.MiddlewareClientID)),
                ClientSecret = appSettings.OrderCloud?.MiddlewareClientSecret ?? throw new ArgumentNullException(nameof(appSettings.OrderCloud.MiddlewareClientSecret)),
                Roles = [ApiRole.FullAccess],
            }))
            .AddSingleton<IDigiKeyClient, DigiKeyClient>()
            .AddSingleton<ExportPipeline>();

        services.AddHostedService<BackgroundProcess>();
    })
    .Build();

await host.RunAsync();
