using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace PropertyBrokers.OrchardCore.WorkflowAdditions.GenerateSecureUrlToken.Services
{
    public class SecureUrlTokenService : ISecureUrlTokenService
    {
        private readonly string _secretKey;

        public SecureUrlTokenService(IOptions<SecureUrlTokenOptions> options)
        {
            _secretKey = options.Value.SecretKey;
        }

        public string GenerateToken(string email, TimeSpan validity)
        {
            var expirationTime = DateTime.UtcNow.Add(validity);
            var tokenData = $"{email}|{expirationTime.Ticks}";
            var tokenWithHash = $"{tokenData}|{ComputeHash(tokenData)}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenWithHash));
        }

        public bool ValidateToken(string token, out string email)
        {
            email = null;
            try
            {
                var tokenData = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var parts = tokenData.Split('|');
                if (parts.Length != 3) return false;

                email = parts[0];
                var expirationTicks = long.Parse(parts[1]);
                var hash = parts[2];

                var reconstructedData = $"{email}|{expirationTicks}";
                if (hash != ComputeHash(reconstructedData)) return false;

                var expirationTime = new DateTime(expirationTicks, DateTimeKind.Utc);
                return expirationTime > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input + _secretKey));
                return Convert.ToBase64String(bytes);
            }
        }
    }

    public interface ISecureUrlTokenService
    {
        string GenerateToken(string email, TimeSpan validity);
        bool ValidateToken(string token, out string email);
    }

    public class SecureUrlTokenOptions
    {
        public string SecretKey { get; set; }
    }
}
