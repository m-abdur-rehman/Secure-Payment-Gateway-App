namespace PaymentGateway.Security
{
    public interface IDataProtectionHelper
    {
        /// <summary>
        /// Protect (encrypt) a plaintext string (PII).
        /// </summary>
        string Protect(string plainText);

        /// <summary>
        /// Unprotect (decrypt) a previously protected string.
        /// Throws if the value cannot be unprotected.
        /// </summary>
        string Unprotect(string protectedText);

        /// <summary>
        /// Returns a masked version for display/logging (e.g. last 4 chars).
        /// </summary>
        string Mask(string plainText, int showLast = 4);
    }
}
