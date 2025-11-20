using System;

namespace PaymentGateway.Models.DTOs
{
    public class TransactionLookupResponse
    {
        public string TransactionId { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Amount in PKR after conversion
        /// </summary>
        public decimal AmountPKR { get; set; }

        /// <summary>
        /// The original currency used by the customer (USD, AED, SAR, etc.)
        /// </summary>
        public string OriginalCurrency { get; set; }

        /// <summary>
        /// Original amount before conversion
        /// </summary>
        public decimal OriginalAmount { get; set; }

        public string Email { get; set; }

        public string MobileNumber { get; set; }
    }
}
