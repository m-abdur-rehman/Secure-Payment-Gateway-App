using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Models
{
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "Bank Account Number is required.")]
        [StringLength(24, MinimumLength = 8, ErrorMessage = "Account Number must be between 8 and 24 characters.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Bank Account Number must contain digits only.")]
        public string BankAccountNumber { get; set; }

        [Required(ErrorMessage = "Bank Name is required.")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(@"^\d{5}-\d{7}-\d{1}$", ErrorMessage = "CNIC must be in xxxxx-xxxxxxx-x format.")]
        public string CNIC { get; set; }

        [Required(ErrorMessage = "Please select a currency.")]
        [RegularExpression("PKR|USD|AED", ErrorMessage = "Invalid currency selected.")]
        public string Currency { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, 100000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required.")]
        [RegularExpression(@"^03[0-9]{9}$", ErrorMessage = "Mobile must be Pakistani format e.g. 03XXXXXXXXX")]
        public string MobileNumber { get; set; }

        public string? Address { get; set; } = "N/A";
    }
}
