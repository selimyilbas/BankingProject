// BankingApp.Application/Services/Implementations/ExchangeRateService.cs
using System;
using System.Threading.Tasks;
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

        public ExchangeRateService(IUnitOfWork unitOfWork, ILogger<ExchangeRateService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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

                // Get latest rate from database
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

                // For now, use hardcoded rates (replace with real API later)
                var hardcodedRate = GetHardcodedRate(fromCurrency, toCurrency);
                
                // Save to database for future use
                await SaveExchangeRateAsync(fromCurrency, toCurrency, hardcodedRate);
                
                return ApiResponse<decimal>.SuccessResponse(hardcodedRate);
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
                // TODO: Implement real exchange rate API integration
                // For now, update with hardcoded values
                
                var currencies = new[] { "TL", "EUR", "USD" };
                
                foreach (var fromCurrency in currencies)
                {
                    foreach (var toCurrency in currencies)
                    {
                        if (fromCurrency != toCurrency)
                        {
                            var rate = GetHardcodedRate(fromCurrency, toCurrency);
                            await SaveExchangeRateAsync(fromCurrency, toCurrency, rate);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates");
            }
        }

        private decimal GetHardcodedRate(string fromCurrency, string toCurrency)
        {
            // Hardcoded rates for development (as of 2024)
            // Replace with real API integration
            return (fromCurrency, toCurrency) switch
            {
                ("USD", "TL") => 32.50m,
                ("TL", "USD") => 0.0308m,
                ("EUR", "TL") => 35.20m,
                ("TL", "EUR") => 0.0284m,
                ("USD", "EUR") => 0.92m,
                ("EUR", "USD") => 1.087m,
                _ => 1.0m
            };
        }

        private async Task SaveExchangeRateAsync(string fromCurrency, string toCurrency, decimal rate)
        {
            var exchangeRate = new ExchangeRateHistory
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                Rate = rate,
                CaptureDate = DateTime.UtcNow,
                Source = "HARDCODED" // Change to actual source when using real API
            };

            await _unitOfWork.ExchangeRates.AddAsync(exchangeRate);
        }
    }
}