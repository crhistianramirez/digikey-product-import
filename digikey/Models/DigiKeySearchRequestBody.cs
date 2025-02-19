namespace ordercloud_bulk_import_console.digikey.Models
{
    public class DigiKeySearchRequestBody
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Keywords { get; set; }
        public DigiKeyFilerOptionsRequest FilterOptionsRequest { get; set; }

    }

    public class DigiKeyFilerOptionsRequest
    {
        public List<DigiKeyCategoryFilter> CategoryFilter { get; set; }
    }

    public class DigiKeyCategoryFilter
    {
        public string Id { get; set; }
    }
}