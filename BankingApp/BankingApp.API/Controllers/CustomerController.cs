using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.Services.Interfaces;
using BankingApp.Application.DTOs.Customer;
using BankingApp.Application.DTOs.Common;

namespace BankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto dto)
        {
            var result = await _customerService.CreateCustomerAsync(dto);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerById(int customerId)
        {
            var result = await _customerService.GetCustomerByIdAsync(customerId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("by-number/{customerNumber}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByNumber(string customerNumber)
        {
            var result = await _customerService.GetCustomerByNumberAsync(customerNumber);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("by-tckn/{tckn}")]
        public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByTCKN(string tckn)
        {
            var result = await _customerService.GetCustomerByTCKNAsync(tckn);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet("{customerId}/with-accounts")]
        public async Task<ActionResult<ApiResponse<CustomerWithAccountsDto>>> GetCustomerWithAccounts(int customerId)
        {
            var result = await _customerService.GetCustomerWithAccountsAsync(customerId);
            if (result.Success)
                return Ok(result);
            return NotFound(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<CustomerSummaryDto>>>> GetAllCustomers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            var result = await _customerService.GetAllCustomersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPost("validate-tckn")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateTCKN([FromBody] ValidateTCKNDto dto)
        {
            var result = await _customerService.ValidateTCKNAsync(dto.TCKN, dto.FirstName, dto.LastName, dto.BirthYear);
            return Ok(result);
        }
    }

    public class ValidateTCKNDto
    {
        public string TCKN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int BirthYear { get; set; }
    }
}