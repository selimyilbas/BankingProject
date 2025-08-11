using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Customer;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// Müşteri yönetimi uç noktaları: kayıt, sorgulama, güncelleme ve şifre işlemleri.
    /// </summary>
    [ApiController]


    // http://localhost:5115/swagger(api)/customer 
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Yeni müşteri oluşturur.
        /// </summary>
        /// <param name="dto">Müşteri oluşturma isteği.</param>
        /// <returns>Oluşturulan müşteriyi içeren ApiResponse.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            var result = await _customerService.CreateCustomerAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Müşteri kimliğine göre müşteri bilgilerini getirir.
        /// </summary>
        /// <param name="customerId">İç sistem müşteri kimliği.</param>
        /// <returns>Müşteri bilgilerini içeren ApiResponse.</returns>
        [HttpGet("{customerId}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(int customerId)
        {
            var result = await _customerService.GetCustomerByIdAsync(customerId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Müşteri numarasına göre müşteri bilgilerini getirir.
        /// </summary>
        /// <param name="customerNumber">Müşteri numarası.</param>
        /// <returns>Müşteri bilgilerini içeren ApiResponse.</returns>
        [HttpGet("by-number/{customerNumber}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByNumber(string customerNumber)
        {
            var result = await _customerService.GetCustomerByNumberAsync(customerNumber);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// TCKN'ye göre müşteri bilgilerini getirir.
        /// </summary>
        /// <param name="tckn">Türkiye Cumhuriyeti Kimlik Numarası.</param>
        /// <returns>Müşteri bilgilerini içeren ApiResponse.</returns>
        [HttpGet("by-tckn/{tckn}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByTCKN(string tckn)
        {
            var result = await _customerService.GetCustomerByTCKNAsync(tckn);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Müşteriyi hesaplarıyla birlikte getirir.
        /// </summary>
        /// <param name="customerId">İç sistem müşteri kimliği.</param>
        /// <returns>Müşteri ve hesap özetini içeren ApiResponse.</returns>
        [HttpGet("{customerId}/with-accounts")]
        public async Task<ActionResult<ApiResponse<CustomerWithAccountsDto>>> GetCustomerWithAccounts(int customerId)
        {
            var result = await _customerService.GetCustomerWithAccountsAsync(customerId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        /// <summary>
        /// Müşteri listesini sayfalı olarak getirir.
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası.</param>
        /// <param name="pageSize">Sayfa başına kayıt sayısı.</param>
        /// <returns>Sayfalı müşteri sonuçlarını içeren ApiResponse.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<CustomerSummaryDto>>>> GetAllCustomers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            var result = await _customerService.GetAllCustomersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Müşteri bilgilerini günceller.
        /// </summary>
        /// <param name="customerId">İç sistem müşteri kimliği.</param>
        /// <param name="dto">Güncellenecek müşteri bilgileri.</param>
        /// <returns>Güncellenmiş müşteriyi içeren ApiResponse.</returns>
        [HttpPut("{customerId}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(int customerId, [FromBody] UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateCustomerAsync(customerId, dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// Müşteri şifresini değiştirir.
        /// </summary>
        /// <param name="customerId">İç sistem müşteri kimliği.</param>
        /// <param name="dto">Mevcut ve yeni şifre bilgisi.</param>
        /// <returns>İşlem sonucunu içeren ApiResponse.</returns>
        [HttpPost("{customerId}/change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(int customerId, [FromBody] ChangePasswordDto dto)
        {
            var result = await _customerService.ChangePasswordAsync(customerId, dto.CurrentPassword, dto.NewPassword);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        /// <summary>
        /// TCKN doğrulaması yapar (öğrenme/test amaçlı).
        /// </summary>
        /// <param name="dto">Kimlik bilgileri.</param>
        /// <returns>Doğrulama sonucunu içeren ApiResponse.</returns>
        [HttpPost("validate-tckn")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateTCKN([FromBody] ValidateTCKNDto dto)
        {
            var result = await _customerService.ValidateTCKNAsync(dto.TCKN, dto.FirstName, dto.LastName, dto.BirthYear);
            return Ok(result);
        }
    }

    /// <summary>
    /// TCKN doğrulama isteği modeli.
    /// </summary>
    public class ValidateTCKNDto
    {
        /// <summary>
        /// Türkiye Cumhuriyeti Kimlik Numarası.
        /// </summary>
        public string TCKN { get; set; } = string.Empty;
        /// <summary>
        /// Ad.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>
        /// Soyad.
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// Doğum yılı.
        /// </summary>
        public int BirthYear { get; set; }
    }
}