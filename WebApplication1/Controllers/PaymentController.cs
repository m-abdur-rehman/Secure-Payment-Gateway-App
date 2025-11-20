using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Models;
using PaymentGateway.Repositories.Interfaces;
using PaymentGateway.Security;
using System.Security.Cryptography;

namespace PaymentGateway.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ITransactionRepository _repo;
        private readonly IDataProtectionHelper _dataProtection;
        private readonly IForexService _forex;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            ITransactionRepository repo,
            IDataProtectionHelper dataProtection,
            IForexService forex,
            ILogger<PaymentController> logger)
        {
            _repo = repo;
            _dataProtection = dataProtection;
            _forex = forex;
            _logger = logger;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Transactions()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            try
            {
                // Generate secure Transaction ID
                var txId = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))
                    .Replace("/", "_").Replace("+", "-");

                // Convert User Amount → PKR via Forex API
                var amountPkr = await _forex.ConvertToPkrAsync(model.Amount, model.Currency);

                var encryptedCnic = _dataProtection.Protect(model.CNIC);
                var encryptedBankAccount = _dataProtection.Protect(model.BankAccountNumber);

                var entity = new Transaction
                {
                    TransactionId = txId,
                    CreatedAt = DateTime.UtcNow,

                    AmountPKR = amountPkr,
                    OriginalAmount = model.Amount,
                    OriginalCurrency = model.Currency,

                    Email = model.Email,
                    Address = model.Address ?? "N/A",
                    BankId = model.BankName,

                    EncryptedCNIC = encryptedCnic,
                    EncryptedBankAccount = encryptedBankAccount,

                    MobileNumber = model.MobileNumber,
                    IsDeleted = false
                };

                await _repo.AddAsync(entity);

                return RedirectToAction("Success", new { id = txId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting payment.");
                ModelState.AddModelError("", "An unexpected error occurred.");
                return View("Index", model);
            }
        }

        public IActionResult Success(string id)
        {
            ViewBag.TxId = id;
            return View();
        }
    }
}
