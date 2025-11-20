using Microsoft.AspNetCore.DataProtection;
using System;

namespace PaymentGateway.Security
{
    public class DataProtectionHelper : IDataProtectionHelper
    {
        private readonly IDataProtector _protector;

        // Use a stable purpose string so protected values can be unprotected by same app
        private const string Purpose = "PaymentGateway.PII.Protection-v1";

        public DataProtectionHelper(IDataProtectionProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            _protector = provider.CreateProtector(Purpose);
        }

        public string Protect(string plainText)
        {
            if (plainText == null) return null!;
            return _protector.Protect(plainText);
        }

        public string Unprotect(string protectedText)
        {
            if (protectedText == null) return null!;
            return _protector.Unprotect(protectedText);
        }

        public string Mask(string plainText, int showLast = 4)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;
            if (plainText.Length <= showLast) return new string('*', plainText.Length);

            var maskLength = plainText.Length - showLast;
            return new string('*', maskLength) + plainText.Substring(maskLength);
        }
    }
}
