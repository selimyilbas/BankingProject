using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BankingApp.Application.Services.Interfaces;

namespace BankingApp.Application.Services.Implementations
{
    public class VakifbankApiService : IVakifbankApiService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VakifbankApiService> _logger;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        private record TokenResponse(string access_token, string token_type, int expires_in, string scope);
        private record CurrencyItem(string CurrencyCode, string RateDate, string PurchaseRate, string SaleRate);
        private record GetCurrencyRatesData(List<CurrencyItem> Currency);
        private record ApiEnvelope<T>(ApiHeader Header, T Data);
        private record ApiHeader(string StatusCode, string StatusDescription, string StatusDescriptionEn, string ObjectID);

        public VakifbankApiService(HttpClient http, IConfiguration config, IMemoryCache cache, ILogger<VakifbankApiService> logger)
        {
            _http = http;
            _config = config;
            _cache = cache;
            _logger = logger;

            var baseUrl = _config["VakifbankApi:BaseUrl"] ?? "https://apigw.vakifbank.com.tr:8443";
            _http.BaseAddress = new Uri(baseUrl);
            if (int.TryParse(_config["VakifbankApi:TimeoutSeconds"], out var timeoutSeconds))
            {
                _http.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            }
        }

        public async Task<string?> GetAccessTokenAsync(string scope = "public")
        {
            var cacheKey = $"vakifbank_token::{scope}";
            if (_cache.TryGetValue(cacheKey, out string token))
                return token;

            var tokenUrl = _config["VakifbankApi:TokenUrl"] ?? "https://apigw.vakifbank.com.tr:8443/auth/oauth/v2/token";
            var clientId = _config["VakifbankApi:ClientId"];
            var clientSecret = _config["VakifbankApi:ClientSecret"];
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                _logger.LogWarning("Vakifbank API credentials are missing.");
                return null;
            }

            var form = new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["grant_type"] = "client_credentials",
                ["scope"] = scope
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(form)
            };
            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token request failed: {Status}", res.StatusCode);
                return null;
            }

            var json = await res.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<TokenResponse>(json, _json);
            if (dto == null || string.IsNullOrWhiteSpace(dto.access_token))
                return null;

            _cache.Set(cacheKey, dto.access_token, TimeSpan.FromSeconds(Math.Max(dto.expires_in - 30, 60)));
            return dto.access_token;
        }

        public async Task<(decimal buy, decimal sell)?> GetTryRatesAsync(string currencyCode, DateTime dateUtc)
        {
            var token = await GetAccessTokenAsync("public");
            if (string.IsNullOrWhiteSpace(token)) return null;

            // Use +03:00 offset for Turkey (no DST handling here for simplicity)
            var local = new DateTimeOffset(dateUtc, TimeSpan.Zero).ToOffset(TimeSpan.FromHours(3));
            var validity = local.ToString("yyyy-MM-dd'T'HH:mm:sszzz");

            var body = new { ValidityDate = validity };
            using var req = new HttpRequestMessage(HttpMethod.Post, "/getCurrencyRates")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var res = await _http.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("getCurrencyRates failed: {Status}", res.StatusCode);
                return null;
            }

            var json = await res.Content.ReadAsStringAsync();
            var env = JsonSerializer.Deserialize<ApiEnvelope<GetCurrencyRatesData>>(json, _json);
            if (env?.Header?.StatusCode != "APIGW000000") return null;

            var item = env.Data?.Currency?.FirstOrDefault(c => string.Equals(c.CurrencyCode, currencyCode, StringComparison.OrdinalIgnoreCase));
            if (item == null) return null;

            if (decimal.TryParse(item.PurchaseRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var buy) &&
                decimal.TryParse(item.SaleRate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sell))
            {
                return (buy, sell);
            }
            return null;
        }
    }
}


