namespace SecureBankingSystem.Services
{
    public interface ISecurityService
    {
        string GenerateToken();
        bool ValidateToken(string token);
        string EncryptData(string data);
        string DecryptData(string encryptedData);
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}