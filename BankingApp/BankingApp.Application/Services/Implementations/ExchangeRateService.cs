// BankingApp.Application/Services/Implementations/ExchangeRateService.cs
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using BankingApp.Application.DTOs.Common;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Domain.Entities;
using BankingApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BankingApp.Application.Services.Implementations
{
    /// <summary>
    /// Döviz kuru sorgulama ve güncelleme işlevlerini sağlar.
    /// </summary>
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IVakifbankApiService? _vakifbankApiService;
        // Primary provider: ExchangeRate-API v6
        private const string EXCHANGE_RATE_API_BASE_URL = "https://v6.exchangerate-api.com/v6";

        private readonly IConfiguration _configuration;
        private readonly string? _exchangeRateApiKey;

        public ExchangeRateService(
            IUnitOfWork unitOfWork,
            ILogger<ExchangeRateService> logger,
            HttpClient httpClient,
            IMemoryCache cache,
            IVakifbankApiService? vakifbankApiService = null,
            IConfiguration? configuration = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpClient = httpClient;
            _cache = cache;
            _vakifbankApiService = vakifbankApiService;
            _configuration = configuration ?? new ConfigurationBuilder().Build();
            _exchangeRateApiKey = _configuration["ExchangeRateApi:ApiKey"];
        }

        /// <summary>
        /// İki para birimi arasındaki kuru döner.
        /// </summary>
        public async Task<ApiResponse<decimal>> GetExchangeRateAsync(string fromCurrency, string toCurrency, bool skipCache = false)
        {
            try
            {
                // If same currency, rate is 1
                if (fromCurrency == toCurrency)
                {
                    return ApiResponse<decimal>.SuccessResponse(1.0m);
                }

                // Always use TRY for API calls (no TL conversion needed anymore)
                var fromCurrencyApi = fromCurrency == "TL" ? "TRY" : fromCurrency;
                var toCurrencyApi = toCurrency == "TL" ? "TRY" : toCurrency;

                // Try to get from real API first
                var apiRate = await GetRateFromApiAsync(fromCurrencyApi, toCurrencyApi, skipCache);
                if (apiRate.HasValue)
                {
                    // Save to database for future use
                    await SaveExchangeRateAsync(fromCurrency, toCurrency, apiRate.Value);
                    return ApiResponse<decimal>.SuccessResponse(apiRate.Value);
                }

                // Only fallback to database if API fails completely
                var rate = await _unitOfWork.ExchangeRates.GetLatestRateAsync(fromCurrency, toCurrency);
                
                if (rate.HasValue)
                {
                    return ApiResponse<decimal>.SuccessResponse(rate.Value);
                }

                // If no rate found, try reverse rate
                var reverseRate = await _unitOfWork.ExchangeRates.GetLatestRateAsync(toCurrency, fromCurrency);
                
                if (reverseRate.HasValue)
                {
                    return ApiResponse<decimal>.SuccessResponse(1 / reverseRate.Value);
                }

                // Return error instead of hardcoded rates
                return ApiResponse<decimal>.ErrorResponse($"Exchange rate not available for {fromCurrency} to {toCurrency}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                return ApiResponse<decimal>.ErrorResponse("Failed to get exchange rate");
            }
        }

        /// <summary>
        /// Verilen tutarı hedef para birimine çevirir.
        /// </summary>
        public async Task<ApiResponse<decimal>> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            try
            {
                var rateResponse = await GetExchangeRateAsync(fromCurrency, toCurrency);
                
                if (!rateResponse.Success)
                {
                    return ApiResponse<decimal>.ErrorResponse("Failed to get exchange rate");
                }

                var convertedAmount = amount * rateResponse.Data;
                return ApiResponse<decimal>.SuccessResponse(convertedAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting amount");
                return ApiResponse<decimal>.ErrorResponse("Failed to convert amount");
            }
        }

        /// <summary>
        /// Kurları harici API'lerden çekerek günceller.
        /// </summary>
        public async Task UpdateExchangeRatesAsync()
        {
            try
            {
                _logger.LogInformation("Starting exchange rates update from API");
                
                var currencies = new[] { "TRY", "EUR", "USD" }; // Changed TL to TRY
                var updatedCount = 0;
                
                foreach (var fromCurrency in currencies)
                {
                    foreach (var toCurrency in currencies)
                    {
                        if (fromCurrency != toCurrency)
                        {
                            try
                            {
                                var apiRate = await GetRateFromApiAsync(fromCurrency, toCurrency);
                                if (apiRate.HasValue)
                                {
                                    await SaveExchangeRateAsync(fromCurrency, toCurrency, apiRate.Value);
                                    updatedCount++;
                                    _logger.LogInformation("Updated {FromCurrency}-{ToCurrency}: {Rate}", 
                                        fromCurrency, toCurrency, apiRate.Value);
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to get rate for {FromCurrency}-{ToCurrency} from API", 
                                        fromCurrency, toCurrency);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to update rate for {FromCurrency}-{ToCurrency}", 
                                    fromCurrency, toCurrency);
                            }
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Exchange rates update completed. Successfully updated {UpdatedCount} rates from API", updatedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates");
            }
        }



        /// <summary>
        /// Sunum için seçili kurların güncel listesini döner.
        /// </summary>
        public async Task<ApiResponse<ExchangeRatesResponseDto>> GetCurrentExchangeRatesAsync(bool skipCache = false)
        {
            try
            {
                _logger.LogInformation("Fetching current exchange rates (skipCache={Skip})", skipCache);

                // Limit to selected currencies for performance
                var desiredCurrencies = new[]
                {
                    "USD","EUR","GBP","CNY","AED"
                };

                var currencyNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["USD"] = "Amerikan Doları",
                    ["EUR"] = "Euro",
                    ["GBP"] = "İngiliz Sterlini",
                    ["CHF"] = "İsviçre Frangı",
                    ["JPY"] = "Japon Yeni",
                    ["CAD"] = "Kanada Doları",
                    ["AUD"] = "Avustralya Doları",
                    ["CNY"] = "Çin Yuanı",
                    ["RUB"] = "Rus Rublesi",
                    ["AED"] = "BAE Dirhemi",
                    ["SAR"] = "Suudi Riyali",
                    ["NOK"] = "Norveç Kronu",
                    ["SEK"] = "İsveç Kronu",
                    ["DKK"] = "Danimarka Kronu",
                    ["KWD"] = "Kuveyt Dinarı",
                    ["QAR"] = "Katar Riyali",
                    ["BHD"] = "Bahreyn Dinarı",
                    ["INR"] = "Hindistan Rupisi",
                    ["SGD"] = "Singapur Doları",
                    ["HKD"] = "Hong Kong Doları",
                    ["NZD"] = "Yeni Zelanda Doları",
                    ["ZAR"] = "Güney Afrika Randı",
                    ["PLN"] = "Polonya Zlotisi",
                    ["RON"] = "Rumen Leyi",
                    ["HUF"] = "Macar Forinti"
                };

                var exchangeRates = new List<ExchangeRateDisplayDto>();
                var spreadPercent = 0.005m; // 0.5% bank spread

                // Eğer tamamen canlı tazeleme istenmişse, dış API'den tek seferde oran haritasını çekip
                // tüm kurları buradan hesaplayalım. Böylece 1 çağrı ile 5 kur hesaplanır ve rate-limit/429 riski azalır.
                Dictionary<string, decimal>? ratesMap = null;
                if (skipCache)
                {
                    ratesMap = await GetRatesMapFromApiV6(true);
                    if (ratesMap == null)
                    {
                        // Kısa bekleme ile bir kez daha dene
                        await Task.Delay(300);
                        ratesMap = await GetRatesMapFromApiV6(true);
                    }
                }

                if (ratesMap != null)
                {
                    if (!ratesMap.TryGetValue("TRY", out var usdToTry))
                    {
                        _logger.LogWarning("Rates map does not contain TRY entry.");
                    }

                    foreach (var code in desiredCurrencies)
                    {
                        try
                        {
                            decimal? rate = null;
                            if (code.Equals("USD", StringComparison.OrdinalIgnoreCase))
                            {
                                if (ratesMap.TryGetValue("TRY", out var tryRate))
                                {
                                    rate = tryRate; // USD->TRY
                                }
                            }
                            else
                            {
                                if (ratesMap.TryGetValue(code, out var fromRate) && ratesMap.TryGetValue("TRY", out var toRate) && fromRate > 0)
                                {
                                    rate = toRate / fromRate; // code->TRY (USD bazlı çapraz)
                                }
                            }

                            if (rate.HasValue)
                            {
                                var buy = Math.Round(rate.Value * (1 - spreadPercent), 4);
                                var sell = Math.Round(rate.Value * (1 + spreadPercent), 4);

                                exchangeRates.Add(new ExchangeRateDisplayDto
                                {
                                    Currency = code,
                                    CurrencyName = currencyNames.TryGetValue(code, out var name) ? name : code,
                                    BuyRate = buy,
                                    SellRate = sell
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Rate not found in map for {Code}/TRY", code);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to compute rate for {Code}/TRY from map", code);
                        }
                    }
                }
                else
                {
                    // Mevcut yöntem: her kur için ayrı istek. Gerekirse birer kez retry yapalım.
                    foreach (var code in desiredCurrencies)
                    {
                        try
                        {
                            decimal? rate = null;
                            for (int attempt = 1; attempt <= 2 && !rate.HasValue; attempt++)
                            {
                                rate = await GetRateFromApiAsync(code, "TRY", skipCache);
                                if (!rate.HasValue)
                                {
                                    await Task.Delay(200 * attempt);
                                }
                            }

                            if (rate.HasValue)
                            {
                                var buy = Math.Round(rate.Value * (1 - spreadPercent), 4);
                                var sell = Math.Round(rate.Value * (1 + spreadPercent), 4);

                                exchangeRates.Add(new ExchangeRateDisplayDto
                                {
                                    Currency = code,
                                    CurrencyName = currencyNames.TryGetValue(code, out var name) ? name : code,
                                    BuyRate = buy,
                                    SellRate = sell
                                });
                            }
                            else
                            {
                                _logger.LogWarning("Rate not found for {Code}/TRY", code);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to get rate for {Code}/TRY", code);
                        }
                    }
                }

                // Harita yaklaşımı başarısız olup liste boş kalırsa, son çare olarak tek tek canlı çağrılarla dene
                if (exchangeRates.Count == 0)
                {
                    foreach (var code in desiredCurrencies)
                    {
                        try
                        {
                            decimal? rate = null;
                            for (int attempt = 1; attempt <= 2 && !rate.HasValue; attempt++)
                            {
                                rate = await GetRateFromApiAsync(code, "TRY", true);
                                if (!rate.HasValue)
                                {
                                    await Task.Delay(200 * attempt);
                                }
                            }

                            if (rate.HasValue)
                            {
                                var buy = Math.Round(rate.Value * (1 - spreadPercent), 4);
                                var sell = Math.Round(rate.Value * (1 + spreadPercent), 4);

                                exchangeRates.Add(new ExchangeRateDisplayDto
                                {
                                    Currency = code,
                                    CurrencyName = currencyNames.TryGetValue(code, out var name) ? name : code,
                                    BuyRate = buy,
                                    SellRate = sell
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Fallback live fetch failed for {Code}/TRY", code);
                        }
                    }
                }

                if (exchangeRates.Count == 0)
                {
                    return ApiResponse<ExchangeRatesResponseDto>.ErrorResponse("Unable to fetch real-time exchange rates");
                }

                var response = new ExchangeRatesResponseDto
                {
                    Rates = exchangeRates,
                    LastUpdated = DateTime.UtcNow
                };

                return ApiResponse<ExchangeRatesResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current exchange rates");
                return ApiResponse<ExchangeRatesResponseDto>.ErrorResponse("Failed to get exchange rates");
            }
        }

        private async Task<Dictionary<string, decimal>?> GetRatesMapFromApiV6(bool skipCache)
        {
            try
            {
                var baseCurrency = "USD";
                var cacheKey = $"rates::EXCHANGERATE_V6::{baseCurrency}";
                if (!skipCache && _cache.TryGetValue(cacheKey, out Dictionary<string, decimal> cached))
                {
                    return cached;
                }

                if (string.IsNullOrWhiteSpace(_exchangeRateApiKey))
                {
                    _logger.LogWarning("ExchangeRate-API v6 key missing in configuration.");
                    return null;
                }

                var url = $"{EXCHANGE_RATE_API_BASE_URL}/{_exchangeRateApiKey}/latest/{baseCurrency}";
                if (skipCache)
                {
                    url += $"?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                }

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ExchangeRate-API v6 map request failed: {Status}", response.StatusCode);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                if (!root.TryGetProperty("conversion_rates", out var ratesElement))
                {
                    _logger.LogWarning("conversion_rates not found in ExchangeRate-API v6 response");
                    return null;
                }

                var map = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in ratesElement.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDecimal(out var val))
                    {
                        map[prop.Name] = val;
                    }
                }

                if (!skipCache)
                {
                    _cache.Set(cacheKey, map, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                    });
                }

                return map;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching rates map from ExchangeRate-API v6");
                return null;
            }
        }

        private async Task<decimal?> GetRateFromApiAsync(string fromCurrency, string toCurrency, bool skipCache = false)
        {
            // 1) Binance public endpoints (priority)
            var binanceRate = await TryGetRateFromBinance(fromCurrency, toCurrency, skipCache);
            if (binanceRate.HasValue)
            {
                return binanceRate.Value;
            }

            // 2) Fallback: VakıfBank (TRY-USD/EUR çiftleri için)
            var vbRate = await TryGetRateFromVakifbank(fromCurrency, toCurrency);
            if (vbRate.HasValue)
            {
                return vbRate.Value;
            }

            return null;
        }

        private async Task<decimal?> TryGetRateFromVakifbank(string fromCurrency, string toCurrency)
        {
            try
            {
                if (_vakifbankApiService == null)
                {
                    return null;
                }

                // Only direct pairs with TRY supported here
                if (string.Equals(fromCurrency, "TRY", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(toCurrency, "USD", StringComparison.OrdinalIgnoreCase) || string.Equals(toCurrency, "EUR", StringComparison.OrdinalIgnoreCase)))
                {
                    var result = await _vakifbankApiService.GetTryRatesAsync(toCurrency, DateTime.UtcNow);
                    if (result.HasValue && result.Value.sell > 0)
                    {
                        // TRY -> FX: use bank SaleRate, so TRY/FX = 1 / sell
                        return 1m / result.Value.sell;
                    }
                }

                if (string.Equals(toCurrency, "TRY", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(fromCurrency, "USD", StringComparison.OrdinalIgnoreCase) || string.Equals(fromCurrency, "EUR", StringComparison.OrdinalIgnoreCase)))
                {
                    var result = await _vakifbankApiService.GetTryRatesAsync(fromCurrency, DateTime.UtcNow);
                    if (result.HasValue)
                    {
                        // FX -> TRY: use bank PurchaseRate
                        return result.Value.buy;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "VakifBank rate fetch failed");
                return null;
            }
        }

        /// <summary>
        /// Binance Spot public endpoint'lerinden fiyatları alır ve USD/EUR/TRY çiftleri için USDT köprüsüyle kur türetir.
        /// </summary>
        private async Task<decimal?> TryGetRateFromBinance(string fromCurrency, string toCurrency, bool skipCache)
        {
            try
            {
                var from = string.Equals(fromCurrency, "TL", StringComparison.OrdinalIgnoreCase) ? "TRY" : fromCurrency.ToUpperInvariant();
                var to = string.Equals(toCurrency, "TL", StringComparison.OrdinalIgnoreCase) ? "TRY" : toCurrency.ToUpperInvariant();

                // Sadece desteklenen fiatlar: USD, EUR, TRY
                bool IsSupported(string c) => c is "USD" or "EUR" or "TRY";
                if (!IsSupported(from) || !IsSupported(to))
                {
                    return null;
                }

                decimal? usdtTry = null;   // 1 USDT kaç TRY
                decimal? eurUsdt = null;   // 1 EUR kaç USDT

                async Task EnsureUsdtTry()
                {
                    if (!usdtTry.HasValue)
                    {
                        usdtTry = await GetBinancePrice("USDTTRY", skipCache);
                    }
                }

                async Task EnsureEurUsdt()
                {
                    if (!eurUsdt.HasValue)
                    {
                        eurUsdt = await GetBinancePrice("EURUSDT", skipCache);
                    }
                }

                if (from == "USD" && to == "TRY")
                {
                    await EnsureUsdtTry();
                    return usdtTry;
                }
                if (from == "TRY" && to == "USD")
                {
                    await EnsureUsdtTry();
                    return usdtTry is > 0 ? 1m / usdtTry.Value : null;
                }
                if (from == "EUR" && to == "USD")
                {
                    await EnsureEurUsdt();
                    return eurUsdt; // EUR -> USD ~ EURUSDT
                }
                if (from == "USD" && to == "EUR")
                {
                    await EnsureEurUsdt();
                    return eurUsdt is > 0 ? 1m / eurUsdt.Value : null; // USD -> EUR ~ 1/(EURUSDT)
                }
                if (from == "EUR" && to == "TRY")
                {
                    await EnsureUsdtTry();
                    await EnsureEurUsdt();
                    if (usdtTry is > 0 && eurUsdt is > 0)
                    {
                        return usdtTry.Value * eurUsdt.Value; // EUR->USDT->TRY
                    }
                    return null;
                }
                if (from == "TRY" && to == "EUR")
                {
                    await EnsureUsdtTry();
                    await EnsureEurUsdt();
                    var denom = (usdtTry ?? 0m) * (eurUsdt ?? 0m);
                    return denom > 0 ? 1m / denom : null; // TRY->USDT->EUR
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Binance rate fetch failed");
                return null;
            }
        }

        /// <summary>
        /// Binance GET /api/v3/ticker/price?symbol=SYMBOL çağrısı ile fiyat çeker.
        /// </summary>
        private async Task<decimal?> GetBinancePrice(string symbol, bool skipCache)
        {
            try
            {
                var cacheKey = $"BINANCE::{symbol}";
                if (!skipCache && _cache.TryGetValue(cacheKey, out decimal cached))
                {
                    return cached;
                }

                var url = $"https://api.binance.com/api/v3/ticker/price?symbol={symbol}";
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Binance request failed for {Symbol} with status: {Status}", symbol, response.StatusCode);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                if (!root.TryGetProperty("price", out var priceElement))
                {
                    return null;
                }

                var priceStr = priceElement.GetString();
                if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var price))
                {
                    if (!skipCache)
                    {
                        _cache.Set(cacheKey, price, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                        });
                    }
                    return price;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing Binance response for {Symbol}", symbol);
                return null;
            }
        }

        private async Task<decimal?> TryGetRateFromApi(string baseUrl, string fromCurrency, string toCurrency, string apiName, bool skipCache)
        {
            try
            {
                // ExchangeRate-API uses USD base in our usage
                var baseCurrency = "USD";
                var cacheKey = $"rates::EXCHANGERATE_V6::{baseCurrency}";

                // Try cache first (short TTL)
                Dictionary<string, decimal>? ratesMap = null;
                if (!skipCache && !_cache.TryGetValue(cacheKey, out ratesMap))
                {
                    string url;
                    if (string.IsNullOrWhiteSpace(_exchangeRateApiKey))
                    {
                        _logger.LogWarning("{ApiName} API key is missing. Set ExchangeRateApi:ApiKey in configuration.", apiName);
                        return null;
                    }
                    // v6 format: https://v6.exchangerate-api.com/v6/{API_KEY}/latest/{BASE}
                    url = $"{baseUrl}/{_exchangeRateApiKey}/latest/{baseCurrency}";
                    _logger.LogInformation("Fetching exchange rates map from {ApiName} (base {BaseCurrency})", apiName, baseCurrency);

                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("{ApiName} request failed with status: {StatusCode}", apiName, response.StatusCode);
                        return null;
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("{ApiName} Response JSON (truncated): {JsonString}", apiName, jsonString.Substring(0, Math.Min(200, jsonString.Length)));

                    using var document = JsonDocument.Parse(jsonString);
                    var root = document.RootElement;
                    JsonElement ratesElement;
                    if (!root.TryGetProperty("conversion_rates", out ratesElement))
                    {
                        _logger.LogWarning("No conversion_rates property found in {ApiName} response", apiName);
                        return null;
                    }

                    ratesMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    foreach (var prop in ratesElement.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDecimal(out var val))
                        {
                            ratesMap[prop.Name] = val;
                        }
                    }

                    // Cache for 5 seconds
                    if (!skipCache)
                    {
                        _cache.Set(cacheKey, ratesMap, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                        });
                    }
                }
                else if (skipCache)
                {
                    // Force refresh: fetch fresh map and ignore cache
                    string url;
                    if (string.IsNullOrWhiteSpace(_exchangeRateApiKey))
                    {
                        _logger.LogWarning("{ApiName} API key is missing. Set ExchangeRateApi:ApiKey in configuration.", apiName);
                        return null;
                    }
                    url = $"{baseUrl}/{_exchangeRateApiKey}/latest/{baseCurrency}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    var response = await _httpClient.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("{ApiName} forced refresh failed with status: {StatusCode}", apiName, response.StatusCode);
                        return null;
                    }
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(jsonString);
                    var root = document.RootElement;
                    JsonElement ratesElement;
                    if (!root.TryGetProperty("conversion_rates", out ratesElement))
                    {
                        _logger.LogWarning("No conversion_rates property found in {ApiName} response", apiName);
                        return null;
                    }
                    ratesMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                    foreach (var prop in ratesElement.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetDecimal(out var val))
                        {
                            ratesMap[prop.Name] = val;
                        }
                    }
                }

                decimal? crossRate = null;
                
                // Handle direct conversions and cross-rates
                if (fromCurrency == baseCurrency)
                {
                    // Direct rate from base currency
                    if (ratesMap != null && ratesMap.TryGetValue(toCurrency, out var toRate))
                        crossRate = toRate;
                }
                else if (toCurrency == baseCurrency)
                {
                    // Inverse rate to base currency
                    if (ratesMap != null && ratesMap.TryGetValue(fromCurrency, out var fromRate) && fromRate > 0)
                        crossRate = 1 / fromRate;
                }
                else
                {
                    // Cross rate via base currency
                    if (ratesMap != null && ratesMap.TryGetValue(fromCurrency, out var fromRate) &&
                        ratesMap.TryGetValue(toCurrency, out var toRate) && fromRate > 0)
                        crossRate = toRate / fromRate;
                }

                if (crossRate.HasValue)
                {
                    _logger.LogInformation("✅ {ApiName} - Cross rate {FromCurrency}-{ToCurrency}: {Rate}", 
                        apiName, fromCurrency, toCurrency, crossRate);
                    return crossRate;
                }
                else
                {
                    _logger.LogWarning("❌ {ApiName} - Currency pair {FromCurrency}-{ToCurrency} not found", 
                        apiName, fromCurrency, toCurrency);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calling {ApiName} for {FromCurrency}-{ToCurrency}", 
                    apiName, fromCurrency, toCurrency);
                return null;
            }
        }

        private async Task SaveExchangeRateAsync(string fromCurrency, string toCurrency, decimal rate)
        {
            var exchangeRate = new ExchangeRateHistory
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rate,
                CaptureDate = DateTime.UtcNow,
                Source = "EXCHANGE_RATE_API"
            };

            await _unitOfWork.ExchangeRates.AddAsync(exchangeRate);
        }
    }
}