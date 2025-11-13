namespace HowazitSurveyService.Services.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}

