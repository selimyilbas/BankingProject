using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// Döviz kuru işlemleri uç noktaları.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRateController> _logger;
        private readonly BankingApp.Application.Services.Interfaces.IVakifbankApiService? _vakifbankApiService;

        public ExchangeRateController(IExchangeRateService exchangeRateService, ILogger<ExchangeRateController> logger, BankingApp.Application.Services.Interfaces.IVakifbankApiService? vakifbankApiService = null)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
            _vakifbankApiService = vakifbankApiService;
        }

        /// <summary>
        /// Güncel döviz kurlarını getirir.
        /// </summary>
        /// <returns>Kur bilgilerini içeren ApiResponse.</returns>
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

        /// <summary>
        /// Tanılama amaçlı: VakıfBank API'sinden bugünün USD/EUR TRY kurlarını doğrudan çeker.
        /// </summary>
        /// <returns>VakıfBank yanıt özetini içeren ApiResponse.</returns>
        [HttpGet("vakifbank/today")]
        public async Task<ActionResult<ApiResponse<object>>> GetVakifbankToday()
        {
            try
            {
                if (_vakifbankApiService == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Vakifbank API service not available"));
                }

                var nowUtc = DateTime.UtcNow;
                var usd = await _vakifbankApiService.GetTryRatesAsync("USD", nowUtc);
                var eur = await _vakifbankApiService.GetTryRatesAsync("EUR", nowUtc);

                var data = new
                {
                    timestampUtc = nowUtc,
                    usd = usd.HasValue ? new { purchase = usd.Value.buy, sale = usd.Value.sell } : null,
                    eur = eur.HasValue ? new { purchase = eur.Value.buy, sale = eur.Value.sell } : null
                };

                return Ok(ApiResponse<object>.SuccessResponse(data, "Fetched directly from VakifBank API"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching VakifBank rates");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Internal server error"));
            }
        }

        /// <summary>
        /// İki para birimi arasındaki kuru getirir.
        /// </summary>
        /// <param name="fromCurrency">Kaynak para birimi (örn. USD).</param>
        /// <param name="toCurrency">Hedef para birimi (örn. TRY).</param>
        /// <returns>Kur bilgisini içeren ApiResponse.</returns>
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

        /// <summary>
        /// Kur bilgilerini harici kaynaktan günceller.
        /// </summary>
        /// <returns>Başarı durumunu içeren ApiResponse.</returns>
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