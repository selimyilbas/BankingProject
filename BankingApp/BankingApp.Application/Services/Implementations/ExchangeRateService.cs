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

namespace BankingApp.Application.Services.Implementations
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExchangeRateService> _logger;
        private readonly HttpClient _httpClient;
        // Primary endpoint - Uses frankfurter.app for real-time currency data
        private const string PRIMARY_API_BASE_URL = "https://api.frankfurter.app/latest";
        // Fallback API endpoint in case the primary API fails
        private const string FALLBACK_API_BASE_URL = "https://api.exchangerate-api.com/v4/latest";

        public ExchangeRateService(IUnitOfWork unitOfWork, ILogger<ExchangeRateService> logger, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<decimal>> GetExchangeRateAsync(string fromCurrency, string toCurrency)
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
                var apiRate = await GetRateFromApiAsync(fromCurrencyApi, toCurrencyApi);
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



        public async Task<ApiResponse<ExchangeRatesResponseDto>> GetCurrentExchangeRatesAsync()
        {
            try
            {
                var exchangeRates = new List<ExchangeRateDisplayDto>();
                bool hasRealRates = false;

                _logger.LogInformation("Fetching current exchange rates directly from API (no cache)");

                // Get USD/TRY rate using the new API system
                try
                {
                    var usdToTryRate = await GetRateFromApiAsync("USD", "TRY");
                    if (usdToTryRate.HasValue)
                    {
                        hasRealRates = true;
                        
                        var spreadPercent = 0.005m; // 0.5% spread
                        var buyRate = usdToTryRate.Value * (1 - spreadPercent);
                        var sellRate = usdToTryRate.Value * (1 + spreadPercent);

                        exchangeRates.Add(new ExchangeRateDisplayDto
                        {
                            Currency = "USD",
                            CurrencyName = "Amerikan Doları",
                            BuyRate = Math.Round(buyRate, 4),
                            SellRate = Math.Round(sellRate, 4)
                        });
                        
                        _logger.LogInformation("✅ Real USD/TRY rate: {Rate}", usdToTryRate);
                    }
                    else
                    {
                        _logger.LogWarning("❌ Could not get USD/TRY rate from any API");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to get USD/TRY rate from API");
                }

                // Get EUR/TRY rate using the new API system
                try
                {
                    var eurToTryRate = await GetRateFromApiAsync("EUR", "TRY");
                    if (eurToTryRate.HasValue)
                    {
                        hasRealRates = true;
                        
                        var spreadPercent = 0.005m; // 0.5% spread
                        var buyRate = eurToTryRate.Value * (1 - spreadPercent);
                        var sellRate = eurToTryRate.Value * (1 + spreadPercent);

                        exchangeRates.Add(new ExchangeRateDisplayDto
                        {
                            Currency = "EUR",
                            CurrencyName = "Euro",
                            BuyRate = Math.Round(buyRate, 4),
                            SellRate = Math.Round(sellRate, 4)
                        });
                        
                        _logger.LogInformation("✅ Real EUR/TRY rate: {Rate}", eurToTryRate);
                    }
                    else
                    {
                        _logger.LogWarning("❌ Could not get EUR/TRY rate from any API");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to get EUR/TRY rate from API");
                }

                // Return error if no rates were fetched
                if (!hasRealRates || exchangeRates.Count == 0)
                {
                    _logger.LogError("❌ All exchange rate API calls failed - NO FALLBACK TO HARDCODED");
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

        private async Task<decimal?> GetRateFromApiAsync(string fromCurrency, string toCurrency)
        {
            // Try primary API first (frankfurter.app)
            var rate = await TryGetRateFromApi(PRIMARY_API_BASE_URL, fromCurrency, toCurrency, "Primary API (frankfurter.app)");
            if (rate.HasValue)
            {
                return rate;
            }

            // Fallback to secondary API (exchangerate-api.com)
            rate = await TryGetRateFromApi(FALLBACK_API_BASE_URL, fromCurrency, toCurrency, "Fallback API (exchangerate-api.com)");
            return rate;
        }

        private async Task<decimal?> TryGetRateFromApi(string baseUrl, string fromCurrency, string toCurrency, string apiName)
        {
            try
            {
                // Use EUR as base for frankfurter.app, USD for exchangerate-api.com
                var baseCurrency = baseUrl.Contains("frankfurter") ? "EUR" : "USD";
                var url = $"{baseUrl}?base={baseCurrency}";
                
                _logger.LogInformation("Fetching exchange rate from {ApiName}: {Url}", apiName, url);

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("{ApiName} request failed with status: {StatusCode}", apiName, response.StatusCode);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("{ApiName} Response JSON: {JsonString}", apiName, jsonString.Substring(0, Math.Min(200, jsonString.Length)));
                
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                
                if (!root.TryGetProperty("rates", out var ratesElement))
                {
                    _logger.LogWarning("No rates property found in {ApiName} response", apiName);
                    return null;
                }

                decimal? crossRate = null;
                
                // Handle direct conversions and cross-rates
                if (fromCurrency == baseCurrency)
                {
                    // Direct rate from base currency
                    if (ratesElement.TryGetProperty(toCurrency, out var toRateElem))
                    {
                        crossRate = toRateElem.GetDecimal();
                    }
                }
                else if (toCurrency == baseCurrency)
                {
                    // Inverse rate to base currency
                    if (ratesElement.TryGetProperty(fromCurrency, out var fromRateElem))
                    {
                        var fromRate = fromRateElem.GetDecimal();
                        if (fromRate > 0)
                        {
                            crossRate = 1 / fromRate;
                        }
                    }
                }
                else
                {
                    // Cross rate via base currency
                    if (ratesElement.TryGetProperty(fromCurrency, out var fromRateElem) &&
                        ratesElement.TryGetProperty(toCurrency, out var toRateElem))
                    {
                        var fromRate = fromRateElem.GetDecimal();
                        var toRate = toRateElem.GetDecimal();
                        if (fromRate > 0)
                        {
                            crossRate = toRate / fromRate;
                        }
                    }
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