using Microsoft.AspNetCore.Mvc;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// Sağlık kontrolü ve basit bağlantı testleri için uç noktalar.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Servisin temel sağlık durumunu döner.
        /// </summary>
        /// <returns>Durum, zaman damgası ve servis adı.</returns>
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new 
            { 
                status = "Healthy", 
                timestamp = DateTime.UtcNow,
                service = "BankingApp API"
            });
        }

        /// <summary>
        /// Basit ping testi.
        /// </summary>
        /// <returns>"pong" dizgesi.</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}