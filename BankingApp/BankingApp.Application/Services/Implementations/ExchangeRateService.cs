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
        private const string EXCHANGE_API_BASE_URL = "https://api.exchangerate-api.com/v4/latest";

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

                // Get USD/TRY rate directly from API - no database fallback
                try
                {
                    var url = $"{EXCHANGE_API_BASE_URL}/USD";
                    var httpResponse = await _httpClient.GetAsync(url);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var jsonString = await httpResponse.Content.ReadAsStringAsync();
                        using var document = JsonDocument.Parse(jsonString);
                        var root = document.RootElement;
                        
                        if (root.TryGetProperty("rates", out var ratesElement) && 
                            ratesElement.TryGetProperty("TRY", out var tryRateElement))
                        {
                            var usdToTryRate = tryRateElement.GetDecimal();
                            hasRealRates = true;
                            
                            var spreadPercent = 0.005m; // 0.5% spread
                            var buyRate = usdToTryRate * (1 - spreadPercent);
                            var sellRate = usdToTryRate * (1 + spreadPercent);

                            exchangeRates.Add(new ExchangeRateDisplayDto
                            {
                                Currency = "USD",
                                CurrencyName = "Amerikan Doları",
                                BuyRate = Math.Round(buyRate, 4),
                                SellRate = Math.Round(sellRate, 4)
                            });
                            
                            _logger.LogInformation("✅ Real USD/TRY rate: {Rate}", usdToTryRate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to get USD/TRY rate from API");
                }

                // Get EUR/TRY rate directly from API - no database fallback
                try
                {
                    var url = $"{EXCHANGE_API_BASE_URL}/EUR";
                    var httpResponse = await _httpClient.GetAsync(url);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var jsonString = await httpResponse.Content.ReadAsStringAsync();
                        using var document = JsonDocument.Parse(jsonString);
                        var root = document.RootElement;
                        
                        if (root.TryGetProperty("rates", out var ratesElement) && 
                            ratesElement.TryGetProperty("TRY", out var tryRateElement))
                        {
                            var eurToTryRate = tryRateElement.GetDecimal();
                            hasRealRates = true;
                            
                            var spreadPercent = 0.005m; // 0.5% spread
                            var buyRate = eurToTryRate * (1 - spreadPercent);
                            var sellRate = eurToTryRate * (1 + spreadPercent);

                            exchangeRates.Add(new ExchangeRateDisplayDto
                            {
                                Currency = "EUR",
                                CurrencyName = "Euro",
                                BuyRate = Math.Round(buyRate, 4),
                                SellRate = Math.Round(sellRate, 4)
                            });
                            
                            _logger.LogInformation("✅ Real EUR/TRY rate: {Rate}", eurToTryRate);
                        }
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
            try
            {
                // Simplified: always fetch USD base for reliable free-tier support
                var url = $"{EXCHANGE_API_BASE_URL}/USD";
                _logger.LogInformation("Fetching exchange rate from: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API request failed with status: {StatusCode}", response.StatusCode);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response JSON: {JsonString}", jsonString.Substring(0, Math.Min(200, jsonString.Length)));
                
                // Parse JSON manually to avoid DTO property mapping issues
                using var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                
                if (root.TryGetProperty("rates", out var ratesElement))
                {
                    decimal? crossRate = null;
                    if (ratesElement.TryGetProperty(toCurrency, out var toRateElem))
                    {
                        var toRate = toRateElem.GetDecimal();
                        if (fromCurrency == "USD")
                        {
                            crossRate = toRate; // USD → X
                        }
                        else if (ratesElement.TryGetProperty(fromCurrency, out var fromRateElem))
                        {
                            var fromRate = fromRateElem.GetDecimal();
                            if (fromRate > 0)
                            {
                                crossRate = toRate / fromRate; // cross via USD
                            }
                        }
                    }
                    if (crossRate.HasValue)
                    {
                        _logger.LogInformation("Cross rate {FromCurrency}-{ToCurrency}: {Rate}", fromCurrency, toCurrency, crossRate);
                        return crossRate;
                    }
                    else
                    {
                        _logger.LogWarning("Currency {ToCurrency} not found in API rates", toCurrency);
                    }
                }
                else
                {
                    _logger.LogWarning("No rates property found in API response");
                }

                _logger.LogWarning("Currency {ToCurrency} not found in API response", toCurrency);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling exchange rate API for {FromCurrency}-{ToCurrency}", 
                    fromCurrency, toCurrency);
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