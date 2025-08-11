using System;

namespace BankingApp.Application.Services.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        bool IsEncrypted(string value);
        string Version { get; }
    }
}


