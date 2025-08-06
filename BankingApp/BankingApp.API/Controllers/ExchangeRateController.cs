using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRateController> _logger;

        public ExchangeRateController(IExchangeRateService exchangeRateService, ILogger<ExchangeRateController> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        [HttpGet("current")]
        public async Task<ActionResult<ApiResponse<ExchangeRatesResponseDto>>> GetCurrentExchangeRates()
        {
            try
            {
                _logger.LogInformation("Getting current exchange rates");
                var result = await _exchangeRateService.GetCurrentExchangeRatesAsync();
                
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current exchange rates");
                var errorResponse = ApiResponse<ExchangeRatesResponseDto>.ErrorResponse("Internal server error");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("rate")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetExchangeRate([FromQuery] string fromCurrency, [FromQuery] string toCurrency)
        {
            try
            {
                if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency))
                {
                    var badRequestResponse = ApiResponse<decimal>.ErrorResponse("FromCurrency and ToCurrency are required");
                    return BadRequest(badRequestResponse);
                }

                _logger.LogInformation("Getting exchange rate from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                var result = await _exchangeRateService.GetExchangeRateAsync(fromCurrency, toCurrency);
                
                if (result.Success)
                    return Ok(result);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange rate");
                var errorResponse = ApiResponse<decimal>.ErrorResponse("Internal server error");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateExchangeRates()
        {
            try
            {
                _logger.LogInformation("Updating exchange rates from API");
                await _exchangeRateService.UpdateExchangeRatesAsync();
                
                var successResponse = ApiResponse<object>.SuccessResponse(null, "Exchange rates updated successfully");
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates");
                var errorResponse = ApiResponse<object>.ErrorResponse("Internal server error");
                return StatusCode(500, errorResponse);
            }
        }
    }
}