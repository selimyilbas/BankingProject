using Microsoft.AspNetCore.Mvc;
using BankingApp.Application.DTOs.Common;
using BankingApp.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using BankingApp.Application.Services.Interfaces;

namespace BankingApp.API.Controllers
{
    /// <summary>
    /// Kimlik doğrulama uç noktaları: giriş ve kayıt işlemleri.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthController> _logger;
        private readonly IEncryptionService? _encryptionService;

        public AuthController(IUnitOfWork unitOfWork, ILogger<AuthController> logger, IEncryptionService? encryptionService = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// TCKN ve şifre ile giriş yapar.
        /// </summary>
        /// <param name="loginDto">Giriş bilgileri.</param>
        /// <returns>Müşteri özet bilgileri ile birlikte ApiResponse.</returns>
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

                // Handle encrypted or plain password (backward compatible)
                bool isValidPassword = false;
                string storedPassword = customer.Password ?? string.Empty;

                if (_encryptionService != null && _encryptionService.IsEncrypted(storedPassword))
                {
                    var decrypted = _encryptionService.Decrypt(storedPassword);
                    isValidPassword = decrypted == loginDto.Password;
                }
                else
                {
                    isValidPassword = storedPassword == loginDto.Password;
                }
                
                if (!isValidPassword)
                {
                    _logger.LogWarning("Login attempt with invalid password for TCKN: {TCKN}", loginDto.TCKN);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Geçersiz TCKN veya şifre"
                    });
                }

                // Lazy migrate: if stored was plain and encryption is available, upgrade to encrypted
                if (_encryptionService != null && !_encryptionService.IsEncrypted(storedPassword))
                {
                    try
                    {
                        customer.Password = _encryptionService.Encrypt(loginDto.Password);
                        _unitOfWork.Customers.Update(customer);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch { /* ignore migration errors to not block login */ }
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

        /// <summary>
        /// Yeni müşteri kaydı oluşturur.
        /// </summary>
        /// <param name="registerDto">Kayıt bilgileri.</param>
        /// <returns>Oluşturulan müşteri bilgileri ile ApiResponse.</returns>
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

                // Encrypt password if encryption service is available
                var password = registerDto.Password;
                if (_encryptionService != null)
                {
                    password = _encryptionService.Encrypt(registerDto.Password);
                }

                // Generate unique customer number
                var customerNumber = await _unitOfWork.Customers.GenerateCustomerNumberAsync();

                var customer = new BankingApp.Domain.Entities.Customer
                {
                    CustomerNumber = customerNumber,
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
                    Message = "Kayıt sırasında bir hata oluştu",
                    Errors = new List<string> { ex.Message, ex.InnerException?.Message ?? string.Empty }
                });
            }
        }
    }

    /// <summary>
    /// Giriş isteği modeli.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Türkiye Cumhuriyeti Kimlik Numarası.
        /// </summary>
        public string TCKN { get; set; } = string.Empty;
        /// <summary>
        /// Şifre.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Kayıt isteği modeli.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// Ad.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>
        /// Soyad.
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// Türkiye Cumhuriyeti Kimlik Numarası.
        /// </summary>
        public string TCKN { get; set; } = string.Empty;
        /// <summary>
        /// Şifre.
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Doğum tarihi.
        /// </summary>
        public DateTime DateOfBirth { get; set; }
        /// <summary>
        /// E-posta adresi (opsiyonel).
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// Telefon numarası (opsiyonel).
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
} 