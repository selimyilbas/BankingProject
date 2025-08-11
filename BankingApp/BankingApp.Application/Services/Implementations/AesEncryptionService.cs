using System;
using System.Security.Cryptography;
using System.Text;
using BankingApp.Application.Services.Interfaces;

namespace BankingApp.Application.Services.Implementations
{
    /// <summary>
    /// AES-GCM tabanlı simetrik şifreleme servisinin uygulaması.
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _keyBytes;
        public string Version { get; }

        /// <summary>
        /// Base64 anahtar ve sürüm bilgisi ile servisi başlatır.
        /// </summary>
        public AesEncryptionService(string base64Key, string version)
        {
            if (string.IsNullOrWhiteSpace(base64Key))
            {
                throw new ArgumentException("Encryption key cannot be null or empty", nameof(base64Key));
            }
            Version = string.IsNullOrWhiteSpace(version) ? "v1" : version;
            _keyBytes = Convert.FromBase64String(base64Key);
            if (_keyBytes.Length != 32)
            {
                throw new ArgumentException("Encryption key must be 32 bytes (256-bit) in base64", nameof(base64Key));
            }
        }

        /// <summary>
        /// Değerin bu sürümle şifrelenmiş olup olmadığını kontrol eder.
        /// </summary>
        public bool IsEncrypted(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.StartsWith(Version + ":", StringComparison.Ordinal);
        }

        /// <summary>
        /// Metni AES-GCM algoritması ile şifreler.
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (plainText == null) plainText = string.Empty;

            var nonce = new byte[12]; // 96-bit nonce for GCM
            RandomNumberGenerator.Fill(nonce);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = new byte[plainBytes.Length];
            var tag = new byte[16]; // 128-bit tag

            using var aesGcm = new AesGcm(_keyBytes);
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

            // Combine: nonce || cipher || tag, then base64 once
            var combined = new byte[nonce.Length + cipherBytes.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(cipherBytes, 0, combined, nonce.Length, cipherBytes.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length + cipherBytes.Length, tag.Length);

            var b64 = Convert.ToBase64String(combined);
            return $"{Version}:{b64}";
        }

        /// <summary>
        /// Şifreli metni çözer; başarısızsa orijinal değeri döner.
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            // Expected format: version:base64(nonce||cipher||tag)
            var sep = cipherText.IndexOf(':');
            if (sep <= 0) return cipherText;
            var version = cipherText.Substring(0, sep);
            if (!string.Equals(version, Version, StringComparison.Ordinal))
            {
                return cipherText;
            }

            var b64 = cipherText.Substring(sep + 1);
            byte[] combined;
            try
            {
                combined = Convert.FromBase64String(b64);
            }
            catch
            {
                return cipherText;
            }

            if (combined.Length < 12 + 16)
            {
                return cipherText;
            }

            var nonce = new byte[12];
            Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
            var tag = new byte[16];
            Buffer.BlockCopy(combined, combined.Length - tag.Length, tag, 0, tag.Length);
            var cipherLen = combined.Length - nonce.Length - tag.Length;
            if (cipherLen < 0) return cipherText;
            var cipherBytes = new byte[cipherLen];
            Buffer.BlockCopy(combined, nonce.Length, cipherBytes, 0, cipherLen);

            var plainBytes = new byte[cipherBytes.Length];
            try
            {
                using var aesGcm = new AesGcm(_keyBytes);
                aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
            }
            catch
            {
                return cipherText;
            }

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}


