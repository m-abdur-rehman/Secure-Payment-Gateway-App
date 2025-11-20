using System.ComponentModel.DataAnnotations;
using WebApplication1.Attributes;

public class PaymentRequest
{
    [Required(ErrorMessage = "Bank account number is required.")]
    [RegularExpression(@"^[0-9]{8,24}$", ErrorMessage = "Bank account number must be 8-24 digits.")]
    public string BankAccountNumber { get; set; } = null!;

    [Required(ErrorMessage = "Bank name is required.")]
    public string BankName { get; set; } = null!;

    [Required(ErrorMessage = "CNIC is required.")]
    [RegularExpression(@"^[0-9]{5}-[0-9]{7}-[0-9]$", ErrorMessage = "CNIC must be in format xxxxx-xxxxxxx-x.")]
    public string CNIC { get; set; } = null!;

    [Required(ErrorMessage = "Currency is required.")]
    [RegularExpression("^(PKR|USD|AED)$", ErrorMessage = "Currency must be PKR, USD, or AED.")]
    public string Currency { get; set; } = null!;

    [Required(ErrorMessage = "Amount is required.")]
    [CurrencyAmountRange]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^(\+92|0092|92)?[0-9]{10}$", ErrorMessage = "Mobile number must be 10 digits. International format: +92XXXXXXXXXX")]
    public string MobileNumber { get; set; } = null!;
    
    public string? Address { get; set; }

    /// <summary>
    /// Idempotency key to prevent duplicate payment submissions
    /// Client should generate a unique key for each payment attempt
    /// </summary>
    [StringLength(128, ErrorMessage = "Idempotency key must be 128 characters or less.")]
    public string? IdempotencyKey { get; set; }
}
