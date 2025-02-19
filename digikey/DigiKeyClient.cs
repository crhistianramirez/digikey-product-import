using digikey;
using digikey.Models;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Newtonsoft;
using Microsoft.Extensions.Options;
using ordercloud_bulk_import_console.digikey.Models;

namespace ordercloud_bulk_import_console.digikey
{
    public interface IDigiKeyClient
    {
        Task<List<DigiKeyProduct>> SearchProducts(string keywords, int limit = 100, int offset = 0);
        Task<DigiKeyTokenResponse> AuthenticateAsync();
        Task<DigiKeyTokenResponse> AuthenticateWithCodeAsync(string code);
        Task<DigiKeyTokenResponse> RefreshTokenAsync();
    }

    public class DigiKeyClient : IDigiKeyClient
    {
        internal static readonly ISerializer Serializer = new NewtonsoftJsonSerializer();
        private readonly DigiKeyClientOptions _options;
        private IFlurlClient ApiClient;
        private IFlurlClient AuthClient;
        private DigiKeyTokenResponse TokenResponse;

        public bool IsAuthenticated => TokenResponse?.AccessToken != null && TokenResponse.ExpiresUtc > DateTime.UtcNow;

        public DigiKeyClient(IOptions<DigiKeyClientOptions> options)
        {
            _options = options.Value;
            ApiClient = new FlurlClient(_options.BaseApiUrl)
                .WithHeader("X-DIGIKEY-Client-Id", _options.ClientId)
                .BeforeCall(EnsureTokenAsync)
                .OnError(ThrowDigiKeyException)
                .WithSettings(settings =>
                {
                    settings.JsonSerializer = new NewtonsoftJsonSerializer();
                });
            AuthClient = new FlurlClient(_options.BaseAuthUrl)
                .WithSettings(settings =>
                {
                    settings.JsonSerializer = new NewtonsoftJsonSerializer();
                });
        }

        public async Task<List<DigiKeyProduct>> SearchProducts(string categoryId, int limit = 50, int offset = 0)
        {
            var request = await ApiClient.Request("products/v4/search/keyword")
                .PostJsonAsync(new DigiKeySearchRequestBody
                {
                    Limit = limit,
                    Offset = offset,
                    Keywords = "",
                    FilterOptionsRequest = new DigiKeyFilerOptionsRequest
                    {
                        CategoryFilter = new List<DigiKeyCategoryFilter>
                        {
                            new DigiKeyCategoryFilter
                            {
                                Id = categoryId
                            }
                        }
                    }
                })
                .ReceiveJson<DigiKeySearchResponseBody>();

            return request.Products;
        }

        public async Task<DigiKeyTokenResponse> AuthenticateAsync()
        {
            var response = await AuthClient.Request("v1/oauth2/token")
                .PostUrlEncodedAsync(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret }
                }).ReceiveJson<DigiKeyOAuthTokenResponse>();

            TokenResponse = new DigiKeyTokenResponse
            {
                AccessToken = response.access_token,
                // a bit arbitrary, but trim 30 seconds off the expiration to allow for latency
                ExpiresUtc = DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in) - TimeSpan.FromSeconds(30),
            };

            return TokenResponse;
        }

        public async Task<DigiKeyTokenResponse> AuthenticateWithCodeAsync(string code)
        {
            var response = await AuthClient.Request("v1/oauth2/token")
                .PostUrlEncodedAsync(new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "redirect_uri", _options.RedirectUri }
                }).ReceiveJson<DigiKeyOAuthTokenResponse>();

            return new DigiKeyTokenResponse
            {
                AccessToken = response.access_token,
                RefreshToken = response.refresh_token,
                // a bit arbitrary, but trim 30 seconds off the expiration to allow for latency
                ExpiresUtc = DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in) - TimeSpan.FromSeconds(30),
            };
        }


        public async Task<DigiKeyTokenResponse> RefreshTokenAsync()
        {
            var response = await AuthClient.Request("/v1/oauth2/token")
                .PostUrlEncodedAsync(new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "refresh_token",  TokenResponse.RefreshToken }
                }).ReceiveJson<DigiKeyOAuthTokenResponse>();

            TokenResponse = new DigiKeyTokenResponse
            {
                AccessToken = response.access_token,
                RefreshToken = response.refresh_token,
                // a bit arbitrary, but trim 30 seconds off the expiration to allow for latency
                ExpiresUtc = DateTime.UtcNow + TimeSpan.FromSeconds(response.expires_in) - TimeSpan.FromSeconds(30)
            };

            return TokenResponse;
        }

        private readonly SemaphoreSlim _authLock = new(1);

        private async Task EnsureTokenAsync(FlurlCall call)
        {
            if (!IsAuthenticated)
            {
                await _authLock.WaitAsync();
                try
                {
                    // expired token? try a refresh
                    if (TokenResponse?.RefreshToken != null && TokenResponse.ExpiresUtc < DateTime.UtcNow)
                    {
                        try
                        {
                            await RefreshTokenAsync();
                        }
                        catch
                        {
                            // if anything goes wrong while refreshing the token, we'll fall back on re-authenticating below
                            throw;
                        }
                    }

                    if (!IsAuthenticated)
                        await AuthenticateAsync().ConfigureAwait(false);
                }

                finally
                {
                    _authLock.Release();
                }
            }

            call.Request.WithOAuthBearerToken(TokenResponse.AccessToken);
        }

        private async Task ThrowDigiKeyException(FlurlCall call)
        {
            if (!(call.Exception is FlurlHttpException fex))
                return;

            var error = await fex.GetResponseJsonAsync<DigiKeyError>();
            if (!string.IsNullOrEmpty(error?.title) || !string.IsNullOrEmpty(error?.detail))
            {
                var errorMessage = $"DigiKey Error ({error?.status} - {error?.title}): {error?.detail}";
                throw new DigiKeyException(fex, errorMessage);
            }
            else
            {
                var errorMessage = $"DigiKey Unknown Error: {fex.Message} {fex.InnerException?.Message}";
                throw new DigiKeyException(fex, errorMessage);
            }
        }
    }
}
