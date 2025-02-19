namespace digikey.Models
{
    public class DigiKeyError
    {
        public string type { get; set; }
        public string title { get; set; }
        public int status { get; set; }
        public string detail { get; set; }
        public string instance { get; set; }
        public string correlationId { get; set; }
        public object errors { get; set; }
    }
}
