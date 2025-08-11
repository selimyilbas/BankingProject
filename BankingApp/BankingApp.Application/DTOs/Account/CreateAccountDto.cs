namespace BankingApp.Application.DTOs.Account
{
    /// <summary>
    /// Hesap oluşturma isteği modeli.
    /// </summary>
    public class CreateAccountDto
    {
        /// <summary>
        /// Hesabın bağlanacağı müşteri kimliği.
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// Para birimi (TL, EUR, USD).
        /// </summary>
        public string Currency { get; set; } = string.Empty;
    }
}
