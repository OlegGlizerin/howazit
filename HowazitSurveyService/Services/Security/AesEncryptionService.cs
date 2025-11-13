using System.Security.Cryptography;
using System.Text;
using HowazitSurveyService.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HowazitSurveyService.Services.Security;

public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<AesEncryptionService> _logger;

    public AesEncryptionService(IOptions<EncryptionOptions> options, ILogger<AesEncryptionService> logger)
    {
        _logger = logger;
        if (options.Value is null)
        {
            throw new ArgumentNullException(nameof(options), "Encryption options must be provided.");
        }

        _key = Convert.FromBase64String(options.Value.Key);
        _iv = Convert.FromBase64String(options.Value.Iv);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var ms = new MemoryStream();
            using var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var sw = new StreamWriter(cryptoStream, Encoding.UTF8);
            sw.Write(plainText);
            sw.Flush();
            cryptoStream.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt value.");
            throw;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            var buffer = Convert.FromBase64String(cipherText);
            using var ms = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cryptoStream, Encoding.UTF8);
            return sr.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt value.");
            throw;
        }
    }
}

