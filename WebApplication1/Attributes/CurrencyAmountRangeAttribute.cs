using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Attributes
{
    /// <summary>
    /// Validates amount based on currency type
    /// </summary>
    public class CurrencyAmountRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Amount is required.");

            if (!(value is decimal amount))
                return new ValidationResult("Amount must be a valid number.");

            // Get the currency from the model
            var currencyProperty = validationContext.ObjectType.GetProperty("Currency");
            if (currencyProperty == null)
                return ValidationResult.Success; // Can't validate without currency

            var currency = currencyProperty.GetValue(validationContext.ObjectInstance) as string;

            // Validate based on currency
            decimal maxAmount = currency?.ToUpperInvariant() switch
            {
                "PKR" => 1000000m,
                "USD" => 3500m,
                "AED" => 13000m,
                _ => 1000000m
            };

            if (amount < 0.01m)
                return new ValidationResult("Amount must be at least 0.01.");

            if (amount > maxAmount)
            {
                var formattedMax = maxAmount.ToString("N0");
                return new ValidationResult($"Amount cannot exceed {currency} {formattedMax}.");
            }

            return ValidationResult.Success;
        }
    }
}

