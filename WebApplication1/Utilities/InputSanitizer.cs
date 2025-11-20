using System.Net;
using System.Text.RegularExpressions;

namespace WebApplication1.Utilities
{
    /// <summary>
    /// </summary>
    public static class InputSanitizer
    {
        // SQL injection patterns (for detection, not prevention - EF Core handles prevention)
        private static readonly string[] SqlInjectionPatterns = {
            @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b)",
            @"(--|;|/\*|\*/|xp_|sp_)",
            @"(\bOR\b.*=.*)",
            @"(\bAND\b.*=.*)"
        };

        // XSS patterns
        private static readonly string[] XssPatterns = {
            @"<script[^>]*>.*?</script>",
            @"<iframe[^>]*>.*?</iframe>",
            @"javascript:",
            @"on\w+\s*=",
            @"<img[^>]*src\s*=\s*['""]javascript:",
            @"<link[^>]*href\s*=\s*['""]javascript:",
            @"<style[^>]*>.*?</style>",
            @"expression\s*\("
        };

        /// <summary>
        /// Sanitize string input by removing potentially dangerous characters
        /// </summary>
        public static string SanitizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Replace("\0", string.Empty);
            input = input.Trim();
            input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);

            return input;
        }

        /// <summary>
        /// Validate and sanitize email address
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && !ContainsDangerousPatterns(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validate CNIC format (xxxxx-xxxxxxx-x)
        /// </summary>
        public static bool IsValidCNIC(string cnic)
        {
            if (string.IsNullOrWhiteSpace(cnic))
                return false;

            var pattern = @"^\d{5}-\d{7}-\d{1}$";
            return Regex.IsMatch(cnic, pattern) && !ContainsDangerousPatterns(cnic);
        }

        /// <summary>
        /// Validate mobile number (International format: +92XXXXXXXXXX or 03XXXXXXXXX)
        /// </summary>
        public static bool IsValidMobileNumber(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return false;

            // Accept international format (+92, 0092, 92) or local format (03) followed by 10 digits
            var pattern = @"^(\+92|0092|92|03)?[0-9]{10}$";
            return Regex.IsMatch(mobile, pattern) && !ContainsDangerousPatterns(mobile);
        }

        /// <summary>
        /// Validate bank account number (8-24 digits, numbers only)
        /// </summary>
        public static bool IsValidBankAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                return false;

            // First check: must contain only digits (no letters, special characters, etc.)
            if (!Regex.IsMatch(accountNumber, @"^\d+$"))
                return false;

            // Second check: must be 8-24 digits in length
            var pattern = @"^\d{8,24}$";
            return Regex.IsMatch(accountNumber, pattern) && !ContainsDangerousPatterns(accountNumber);
        }

        /// <summary>
        /// Check if input contains dangerous patterns (SQL injection, XSS)
        /// </summary>
        public static bool ContainsDangerousPatterns(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var lowerInput = input.ToLowerInvariant();

            // Check for SQL injection patterns
            foreach (var pattern in SqlInjectionPatterns)
            {
                if (Regex.IsMatch(lowerInput, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            // Check for XSS patterns
            foreach (var pattern in XssPatterns)
            {
                if (Regex.IsMatch(lowerInput, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// HTML encode output to prevent XSS
        /// </summary>
        public static string HtmlEncode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return WebUtility.HtmlEncode(input);
        }

        /// <summary>
        /// Validate currency code
        /// </summary>
        public static bool IsValidCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return false;

            var validCurrencies = new[] { "PKR", "USD", "AED" };
            return validCurrencies.Contains(currency.ToUpperInvariant()) && 
                   !ContainsDangerousPatterns(currency);
        }

        /// <summary>
        /// Validate amount (positive decimal)
        /// </summary>
        public static bool IsValidAmount(decimal amount)
        {
            return amount >= 0.01m && amount <= 100000000m; // Between 0.01 and 100 million
        }

        /// <summary>
        /// Validate amount based on currency type
        /// </summary>
        public static bool IsValidAmountForCurrency(decimal amount, string currency)
        {
            if (amount < 0.01m)
                return false;

            return currency?.ToUpperInvariant() switch
            {
                "PKR" => amount <= 1000000m,  // Max 1,000,000 PKR
                "USD" => amount <= 3500m,     // Max 3,500 USD
                "AED" => amount <= 13000m,    // Max 13,000 AED
                _ => amount <= 1000000m        // Default to PKR limit
            };
        }

        /// <summary>
        /// Get maximum amount for currency
        /// </summary>
        public static decimal GetMaxAmountForCurrency(string currency)
        {
            return currency?.ToUpperInvariant() switch
            {
                "PKR" => 1000000m,
                "USD" => 3500m,
                "AED" => 13000m,
                _ => 1000000m
            };
        }
    }
}

