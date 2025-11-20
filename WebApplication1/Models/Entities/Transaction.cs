public class Transaction
{
    public int Id { get; set; }
    public string TransactionId { get; set; } = null!;
    public string? IdempotencyKey { get; set; } // For preventing duplicate payments
    public DateTime CreatedAt { get; set; }
    public decimal AmountPKR { get; set; }
    public decimal OriginalAmount { get; set; }
    public string OriginalCurrency { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string BankId { get; set; } = null!; // Bank Name or code
    public string EncryptedCNIC { get; set; } = null!; // encrypted CNIC
    public string EncryptedBankAccount { get; set; } = null!; // encrypted bank account
    public string MobileNumber { get; set; } = null!;
    public bool IsDeleted { get; set; } = false;
}
