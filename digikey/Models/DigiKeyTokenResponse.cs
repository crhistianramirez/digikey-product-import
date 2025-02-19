namespace ordercloud_bulk_import_console.digikey.Models
{
    public class DigiKeyTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresUtc { get; set; }
    }
}
