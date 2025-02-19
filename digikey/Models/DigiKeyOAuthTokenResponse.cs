namespace ordercloud_bulk_import_console.digikey.Models
{
    public class DigiKeyOAuthTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; } // remaining lifetime of the token in seconds
        public string refresh_token { get; set; }
        public string refresh_token_expires_in { get; set; }
        public string token_type { get; set; }
    }
}
