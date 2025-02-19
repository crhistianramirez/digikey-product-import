using Flurl.Http;

namespace digikey
{
    public class DigiKeyException : Exception
    {
        public int? StatusCode { get; }
        public string? RequestUrl { get; }
        public string? RequestBody { get; }
        public string? ResponseBody { get; }

        public DigiKeyException(FlurlHttpException ex, string message) : base(message)
        {
            StatusCode = ex.Call?.Response?.StatusCode;
            RequestUrl = ex.Call?.Request?.Url.ToString();
            RequestBody = ex.Call?.RequestBody;
            ResponseBody = ex.GetResponseStringAsync().GetAwaiter().GetResult();
        }
    }
}
