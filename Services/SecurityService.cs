using Microsoft.AspNetCore.DataProtection;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SecureBankingSystem.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDataProtector _protector;

        public SecurityService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _protector = _dataProtectionProvider.CreateProtector("SecureBankingSystem.SecurityService");
        }

        public string GenerateToken()
        {
            // Generate a secure random token (e.g., for email verification or 2FA)
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public bool ValidateToken(string token)
        {
            // In a real application, you would validate the token against what's stored
            // For this demo, we'll just check that it's a valid Base64 string
            try
            {
                Convert.FromBase64String(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string EncryptData(string data)
        {
            return _protector.Protect(data);
        }

        public string DecryptData(string encryptedData)
        {
            return _protector.Unprotect(encryptedData);
        }

        public string HashPassword(string password)
        {
            // For actual password hashing, you'd use ASP.NET Core Identity
            // This is just for demonstration of the concept
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            // Again, in practice you'd use ASP.NET Core Identity
            // This is just for demonstration
            var newHashedPassword = HashPassword(providedPassword);
            return hashedPassword == newHashedPassword;
        }
    }
}