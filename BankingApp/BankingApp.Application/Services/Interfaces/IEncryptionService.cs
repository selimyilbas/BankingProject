using System;

namespace BankingApp.Application.Services.Interfaces
{
    /// <summary>
    /// Simetrik şifreleme hizmeti sözleşmesi.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Düz metni şifreler.
        /// </summary>
        string Encrypt(string plainText);
        /// <summary>
        /// Şifreli metni çözer.
        /// </summary>
        string Decrypt(string cipherText);
        /// <summary>
        /// Değerin belirtilen sürüm formatında şifreli olup olmadığını kontrol eder.
        /// </summary>
        bool IsEncrypted(string value);
        /// <summary>
        /// Şifreleme sürüm bilgisi.
        /// </summary>
        string Version { get; }
    }
}


