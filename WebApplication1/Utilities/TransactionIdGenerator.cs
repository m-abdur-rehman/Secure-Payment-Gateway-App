using System.Security.Cryptography;

public static class TransactionIdGenerator
{
    public static string Generate()
    {
        var random = RandomNumberGenerator.GetBytes(9); // 9 bytes random
        var token = Convert.ToBase64String(random)
            .Replace("+", "").Replace("/", "").Replace("=", "");
        return $"T{DateTime.UtcNow:yyyyMMddHHmmss}-{token}";
    }
}
