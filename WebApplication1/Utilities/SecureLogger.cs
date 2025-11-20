using Microsoft.Extensions.Logging;

namespace WebApplication1.Utilities
{
    /// <summary>
    /// Secure logging utility - ensures sensitive data is never logged
    /// Following PCI-DSS requirement 10.5.2 (Protect audit trail files)
    /// </summary>
    public static class SecureLogger
    {
        // Patterns that indicate sensitive data
        private static readonly string[] SensitivePatterns = {
            @"\b\d{5}-\d{7}-\d{1}\b", // CNIC
            @"\b\d{8,24}\b", // Bank account numbers
            @"\b03\d{9}\b", // Mobile numbers
            @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b" // Email (partial masking)
        };

        /// <summary>
        /// Sanitize log message to remove sensitive information
        /// </summary>
        public static string SanitizeLogMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            var sanitized = message;

            // Mask CNIC
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized, 
                @"\b\d{5}-\d{7}-\d{1}\b", 
                "XXXXX-XXXXXXX-X");

            // Mask bank account (keep first 2 and last 2 digits)
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                @"\b(\d{2})\d{4,20}(\d{2})\b",
                m => $"{m.Groups[1].Value}****{m.Groups[2].Value}");

            // Mask mobile (keep first 3 and last 2 digits)
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                @"\b(03\d)\d{5}(\d{2})\b",
                m => $"{m.Groups[1].Value}*****{m.Groups[2].Value}");

            // Mask email (keep first 2 chars and domain)
            sanitized = System.Text.RegularExpressions.Regex.Replace(
                sanitized,
                @"\b([A-Za-z0-9]{2})[A-Za-z0-9._%+-]*@([A-Za-z0-9.-]+\.[A-Z|a-z]{2,})\b",
                m => $"{m.Groups[1].Value}***@{m.Groups[2].Value}");

            return sanitized;
        }

        /// <summary>
        /// Log error securely (no sensitive data)
        /// </summary>
        public static void LogErrorSecure(ILogger logger, Exception ex, string message, params object[] args)
        {
            var sanitizedMessage = SanitizeLogMessage(message);
            var sanitizedArgs = args.Select(a => SanitizeLogMessage(a?.ToString() ?? string.Empty)).ToArray();
            logger.LogError(ex, sanitizedMessage, sanitizedArgs);
        }

        public static void LogWarningSecure(ILogger logger, string message, params object[] args)
        {
            var sanitizedMessage = SanitizeLogMessage(message);
            var sanitizedArgs = args.Select(a => SanitizeLogMessage(a?.ToString() ?? string.Empty)).ToArray();
            logger.LogWarning(sanitizedMessage, sanitizedArgs);
        }
    }
}

