using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;
using PaymentGateway.Models.DTOs;
using PaymentGateway.Repositories.Interfaces;
using PaymentGateway.Security;
using WebApplication1.Attributes;
using WebApplication1.Utilities;
using static WebApplication1.Utilities.SecureLogger;


namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ValidateInput]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionRepository _repo;
        private readonly IForexService _forex;
        private readonly IDataProtectionHelper _dataProtectionHelper;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionRepository repo,
            IForexService forex,
            IDataProtectionHelper dataProtectionHelper,
            ILogger<TransactionsController> logger)
        {
            _repo = repo;
            _forex = forex;
            _dataProtectionHelper = dataProtectionHelper;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] PaymentRequest req)
        {
            if (req == null)
                return BadRequest(new { error = "Request body is required." });
            if (!InputSanitizer.IsValidEmail(req.Email))
                ModelState.AddModelError(nameof(req.Email), "Invalid email format.");

            if (!InputSanitizer.IsValidCNIC(req.CNIC))
                ModelState.AddModelError(nameof(req.CNIC), "Invalid CNIC format.");

            if (!InputSanitizer.IsValidMobileNumber(req.MobileNumber))
                ModelState.AddModelError(nameof(req.MobileNumber), "Invalid mobile number format.");

            // Validate bank account number - must be only digits
            if (string.IsNullOrWhiteSpace(req.BankAccountNumber))
            {
                ModelState.AddModelError(nameof(req.BankAccountNumber), "Bank account number is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(req.BankAccountNumber, @"^\d+$"))
            {
                ModelState.AddModelError(nameof(req.BankAccountNumber), "Bank account number must contain only numbers.");
            }
            else if (!InputSanitizer.IsValidBankAccount(req.BankAccountNumber))
            {
                ModelState.AddModelError(nameof(req.BankAccountNumber), "Bank account number must be 8-24 digits.");
            }

            if (!InputSanitizer.IsValidCurrency(req.Currency))
                ModelState.AddModelError(nameof(req.Currency), "Invalid currency.");

            if (!InputSanitizer.IsValidAmountForCurrency(req.Amount, req.Currency))
            {
                var maxAmount = InputSanitizer.GetMaxAmountForCurrency(req.Currency);
                var formattedMax = maxAmount.ToString("N0");
                ModelState.AddModelError(nameof(req.Amount), 
                    $"Amount cannot exceed {req.Currency} {formattedMax}.");
            }

            // Check for dangerous patterns
            if (InputSanitizer.ContainsDangerousPatterns(req.BankName))
                ModelState.AddModelError(nameof(req.BankName), "Invalid bank name.");

            if (!ModelState.IsValid)
            {
                var errors = new Dictionary<string, List<string>>();
                foreach (var key in ModelState.Keys)
                {
                    var errorMessages = ModelState[key].Errors
                        .Select(e => e.ErrorMessage)
                        .Where(msg => !string.IsNullOrWhiteSpace(msg))
                        .ToList();
                    
                    if (errorMessages.Any())
                    {
                        errors[key] = errorMessages;
                    }
                }
                
                return BadRequest(new { 
                    error = "Validation failed. Please check your input.",
                    errors = errors
                });
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(req.IdempotencyKey))
                {
                    var existingTx = await _repo.FindByIdempotencyKeyAsync(req.IdempotencyKey);
                    if (existingTx != null)
                    {
                        var existingResponse = new PaymentResponse
                        {
                            TransactionId = existingTx.TransactionId,
                            CreatedAt = existingTx.CreatedAt
                        };
                        return Ok(existingResponse);
                    }
                }

                var amountPkr = await _forex.ConvertToPkrAsync(req.Amount, req.Currency);

                var protectedCnic = _dataProtectionHelper.Protect(req.CNIC);
                var protectedBankAccount = _dataProtectionHelper.Protect(req.BankAccountNumber);

                var txId = TransactionIdGenerator.Generate();
                var normalizedMobile = NormalizeMobileNumber(req.MobileNumber);

                var tx = new Transaction
                {
                    TransactionId = txId,
                    IdempotencyKey = req.IdempotencyKey,
                    CreatedAt = DateTime.UtcNow,
                    AmountPKR = amountPkr,
                    OriginalAmount = req.Amount,
                    OriginalCurrency = req.Currency,
                    Email = req.Email,
                    Address = req.Address ?? "N/A",
                    BankId = req.BankName,
                    EncryptedCNIC = protectedCnic,
                    EncryptedBankAccount = protectedBankAccount,
                    MobileNumber = normalizedMobile
                };

                await _repo.AddAsync(tx);

                var response = new PaymentResponse
                {
                    TransactionId = txId,
                    CreatedAt = tx.CreatedAt
                };

                return CreatedAtAction(nameof(Get),
                    new { transactionId = txId, mobile = tx.MobileNumber },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                LogErrorSecure(_logger, ex, "Forex conversion error. RequestId: {RequestId}", 
                    HttpContext.TraceIdentifier);
                
                return StatusCode(500, new { 
                    error = "Currency conversion failed. Please check your configuration or try again later.",
                    details = ex.Message
                });
            }
            catch (HttpRequestException ex)
            {
                LogErrorSecure(_logger, ex, "Forex API request failed. RequestId: {RequestId}", 
                    HttpContext.TraceIdentifier);
                
                return StatusCode(503, new { 
                    error = "Unable to connect to currency conversion service. Please try again later."
                });
            }
            catch (Exception ex)
            {
                LogErrorSecure(_logger, ex, "Error while creating a transaction. RequestId: {RequestId}", 
                    HttpContext.TraceIdentifier);
                
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            try
            {
                var (items, total) = await _repo.GetPagedAsync(page, pageSize);

                var result = items.Select(tx => new
                {
                    transactionId = tx.TransactionId,
                    createdAt = tx.CreatedAt,
                    amountPKR = tx.AmountPKR,
                    email = tx.Email,
                    mobileNumber = tx.MobileNumber
                }).ToList();

                return Ok(new
                {
                    items = result,
                    total = total,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                LogErrorSecure(_logger, ex, "Error retrieving paginated transactions. RequestId: {RequestId}", 
                    HttpContext.TraceIdentifier);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> Get(string transactionId, [FromQuery] string mobile)
        {
            if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(mobile))
                return BadRequest(new { error = "TransactionId and mobile are required." });

            transactionId = InputSanitizer.SanitizeString(transactionId);
            mobile = InputSanitizer.SanitizeString(mobile);

            if (!InputSanitizer.IsValidMobileNumber(mobile))
                return BadRequest(new { error = "Invalid mobile number format." });

            if (InputSanitizer.ContainsDangerousPatterns(transactionId) || 
                InputSanitizer.ContainsDangerousPatterns(mobile))
                return BadRequest(new { error = "Invalid input detected." });

            try
            {
                var tx = await _repo.FindByTransactionIdAsync(transactionId);
                if (tx == null)
                    return NotFound(new { error = "Transaction not found." });

                if (!SecureCompare(tx.MobileNumber, mobile))
                {
                    LogWarningSecure(_logger, "Transaction lookup failed: Mobile number mismatch. TransactionId: {TxId}", 
                        transactionId);
                    return Forbid();
                }

                var resp = new TransactionLookupResponse
                {
                    TransactionId = tx.TransactionId,
                    CreatedAt = tx.CreatedAt,
                    AmountPKR = tx.AmountPKR,
                    OriginalCurrency = tx.OriginalCurrency,
                    OriginalAmount = tx.OriginalAmount,
                    Email = tx.Email,
                    MobileNumber = tx.MobileNumber
                };

                return Ok(resp);
            }
            catch (Exception ex)
            {
                LogErrorSecure(_logger, ex, "Error retrieving transaction. RequestId: {RequestId}", 
                    HttpContext.TraceIdentifier);
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }

        private string NormalizeMobileNumber(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return mobile;

            var digits = System.Text.RegularExpressions.Regex.Replace(mobile, @"\D", "");

            if (digits.StartsWith("92") && digits.Length == 12)
            {
                digits = digits.Substring(2);
            }
            else if (digits.StartsWith("0") && digits.Length == 11)
            {
                digits = digits.Substring(1);
            }

            if (digits.Length == 10)
            {
                return "+92" + digits;
            }

            if (mobile.StartsWith("+92") && mobile.Length == 13)
            {
                return mobile;
            }

            return mobile;
        }

        private bool SecureCompare(string a, string b)
        {
            if (a == null || b == null)
                return false;

            var normalizedA = NormalizeMobileNumber(a);
            var normalizedB = NormalizeMobileNumber(b);

            if (normalizedA.Length != normalizedB.Length)
                return false;

            int result = 0;
            for (int i = 0; i < normalizedA.Length; i++)
            {
                result |= char.ToUpperInvariant(normalizedA[i]) ^ char.ToUpperInvariant(normalizedB[i]);
            }
            return result == 0;
        }
    }
}
