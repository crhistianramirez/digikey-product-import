namespace ordercloud_bulk_import_console
{
    public class AppSettings
    {
        public OrderCloudAppSettings? OrderCloud { get; set; }
        public DigiKeyAppSettings? DigiKey { get; set; }
    }

    public class OrderCloudAppSettings
    {
        public string? ApiUrl { get; set; }
        public string? MiddlewareClientID { get; set; }
        public string? MiddlewareClientSecret { get; set; }
        public string? CatalogId { get; set; }
    }

    public class DigiKeyAppSettings
    {
        public string BaseAuthUrl { get; set; }
        public string BaseApiUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
