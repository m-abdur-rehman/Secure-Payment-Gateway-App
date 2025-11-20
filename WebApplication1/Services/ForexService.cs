using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class ForexService : IForexService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ForexService> _logger;

    public ForexService(HttpClient http, IConfiguration config, IMemoryCache cache, ILogger<ForexService> logger)
    {
        _http = http;
        _config = config;
        _cache = cache;
        _logger = logger;
    }

    public async Task<decimal> ConvertToPkrAsync(decimal amount, string currency)
    {
        currency = currency.ToUpper();

        if (currency == "PKR")
            return amount;

        var cacheKey = $"rate_{currency}_PKR";

        var rate = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var baseUrl = _config["Forex:ApiBaseUrl"];
            var apiKey = _config["Forex:ApiKey"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogError("Forex API base URL is not configured.");
                throw new InvalidOperationException("Forex API configuration is missing. Please check your configuration.");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Forex API key is not configured.");
                throw new InvalidOperationException("Forex API key is missing. Please configure it in User Secrets (Development) or appsettings.json (Production).");
            }

            var url = $"{baseUrl}/latest?base={currency}&symbols=PKR";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("apikey", apiKey);

            try
            {
                var res = await _http.SendAsync(req);
                
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError("Forex API returned error: {StatusCode}", res.StatusCode);
                    throw new HttpRequestException($"Forex API error: {res.StatusCode}.");
                }

                using var stream = await res.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("rates", out var ratesElement))
                {
                    _logger.LogError("Forex API response missing 'rates' property.");
                    throw new InvalidOperationException("Forex API response is invalid: missing 'rates' property.");
                }

                if (!ratesElement.TryGetProperty("PKR", out var pkrRate))
                {
                    _logger.LogError("PKR rate not found in Forex API response.");
                    throw new InvalidOperationException("PKR exchange rate not found in API response.");
                }

                return pkrRate.GetDecimal();
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Forex API: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to convert {currency} to PKR. Please try again later.", ex);
            }
        });

        return Math.Round(amount * rate, 2);
    }
}
