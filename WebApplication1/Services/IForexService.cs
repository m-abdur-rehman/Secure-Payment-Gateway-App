public interface IForexService
{
    Task<decimal> ConvertToPkrAsync(decimal amount, string currency);
}
