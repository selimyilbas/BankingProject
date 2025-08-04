using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.DTOs.Common;
using BankingApp.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BankingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUnitOfWork unitOfWork, ILogger<AuthController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for TCKN: {TCKN}", loginDto.TCKN);
                
                // Validate input
                if (string.IsNullOrEmpty(loginDto.TCKN) || string.IsNullOrEmpty(loginDto.Password))
                {
                    _logger.LogWarning("Login attempt with empty TCKN or password");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "TCKN ve şifre gereklidir"
                    });
                }

                // Find customer by TCKN
                _logger.LogInformation("Attempting to find customer by TCKN: {TCKN}", loginDto.TCKN);
                var customer = await _unitOfWork.Customers.GetByTCKNAsync(loginDto.TCKN);
                
                if (customer == null)
                {
                    _logger.LogWarning("Customer not found for TCKN: {TCKN}", loginDto.TCKN);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Geçersiz TCKN veya şifre"
                    });
                }

                _logger.LogInformation("Customer found: {CustomerId}, Password length: {PasswordLength}", 
                    customer.CustomerId, customer.Password?.Length ?? 0);

                // Check if customer is active
                if (!customer.IsActive)
                {
                    _logger.LogWarning("Login attempt for inactive customer: {TCKN}", loginDto.TCKN);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Hesabınız aktif değil"
                    });
                }

                // Simple password comparison
                var isValidPassword = customer.Password == loginDto.Password;
                
                if (!isValidPassword)
                {
                    _logger.LogWarning("Login attempt with invalid password for TCKN: {TCKN}", loginDto.TCKN);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Geçersiz TCKN veya şifre"
                    });
                }

                // Return customer info in exact format
                var response = new
                {
                    customerId = customer.CustomerId,
                    customerNumber = customer.CustomerNumber,
                    firstName = customer.FirstName,
                    lastName = customer.LastName,
                    tckn = customer.TCKN
                };

                _logger.LogInformation("Successful login for customer: {CustomerNumber}", customer.CustomerNumber);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for TCKN: {TCKN}", loginDto.TCKN);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Giriş sırasında bir hata oluştu"
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(registerDto.TCKN) || string.IsNullOrEmpty(registerDto.Password))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "TCKN ve şifre gereklidir"
                    });
                }

                // Check if customer already exists
                var existingCustomer = await _unitOfWork.Customers.GetByTCKNAsync(registerDto.TCKN);
                if (existingCustomer != null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Bu TCKN ile kayıtlı bir müşteri zaten mevcut"
                    });
                }

                // Use plain password for testing
                var password = registerDto.Password;

                // Create new customer
                var customer = new BankingApp.Domain.Entities.Customer
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    TCKN = registerDto.TCKN,
                    Password = password,
                    DateOfBirth = registerDto.DateOfBirth,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // Add customer to database
                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("New customer registered: {CustomerNumber}", customer.CustomerNumber);

                var response = new
                {
                    customerId = customer.CustomerId,
                    customerNumber = customer.CustomerNumber,
                    firstName = customer.FirstName,
                    lastName = customer.LastName,
                    tckn = customer.TCKN,
                    email = customer.Email,
                    phoneNumber = customer.PhoneNumber,
                    dateOfBirth = customer.DateOfBirth,
                    isActive = customer.IsActive,
                    createdDate = customer.CreatedDate
                };

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Kayıt başarılı",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for TCKN: {TCKN}", registerDto.TCKN);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Kayıt sırasında bir hata oluştu"
                });
            }
        }
    }

    public class LoginDto
    {
        public string TCKN { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string TCKN { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
} 