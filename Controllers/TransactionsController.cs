using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BankApp.Helpers;
using BankApp.Data;
using BankApp.Models;
using BankApp.Enums;
using System.Security.Principal;

namespace BankApp.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly BankDbContext _context;

        public TransactionsController(BankDbContext context)
        {
            _context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(string searchString, string transactionTypeFilter, int? pageSizeID, int? page)
        {
            // Query with account included
            var query = _context.Transactions
                .Include(t => t.Account)
                .AsQueryable();

            // Filter: by transaction number
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(t => t.TransactionNumber.Contains(searchString));
            }

            // Filter: by transaction type
            if (!string.IsNullOrWhiteSpace(transactionTypeFilter) &&
                Enum.TryParse(transactionTypeFilter, out TransactionType selectedType))
            {
                query = query.Where(t => t.TransactionType == selectedType);
            }

            // Sort: latest first
            query = query.OrderByDescending(t => t.Timestamp);

            // Page size setup with cookie fallback
            var pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, nameof(Transaction));
            ViewData["pageSizeID"] = pageSize;
            ViewData["PageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            // Maintain filters for UI
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentType"] = transactionTypeFilter;
            ViewData["TransactionTypeFilter"] = new SelectList(Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>());

            // Paginate
            var paginatedList = await PaginatedList<Transaction>.CreateAsync(
                query.AsNoTracking(),
                page ?? 1,
                pageSize
            );

            return View(paginatedList);
        }


        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create(int? accountId)
        {
            List<SelectListItem> accounts;

            if (accountId != null)
            {
                accounts = _context.Accounts
                    .Where(a => a.AccountId == accountId)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountId.ToString(),
                        Text = a.AccountNumber
                    }).ToList();
            }
            else
            {
                accounts = _context.Accounts
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccountId.ToString(),
                        Text = a.AccountNumber
                    }).ToList();
            }

            ViewData["AccountId"] = accounts;

            // Add TransactionType dropdown
            ViewData["TransactionType"] = Enum.GetValues(typeof(TransactionType))
                .Cast<TransactionType>()
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = t.ToString()
                }).ToList();

            return View();
        }



        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,Timestamp,Amount,TransactionType,Description,AccountId")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {

                // Generate a unique 10-digit Account Number
                string newTransactionNumber;
                do
                {
                    newTransactionNumber = GenerateRandomNumber(10);
                }
                while (_context.Transactions.Any(t => t.TransactionNumber == newTransactionNumber));

                transaction.TransactionNumber = newTransactionNumber;

                var account = await _context.Accounts.FindAsync(transaction.AccountId);

                if (account == null)
                {
                    ModelState.AddModelError("", "Invalid account.");
                    RebuildDropdowns(transaction);
                    return View(transaction);
                }

                if (account.Status == AccountStatus.Frozen || account.Status == AccountStatus.Closed)
                {
                    ModelState.AddModelError("", $"Transactions are not allowed on {account.Status} accounts.");
                    RebuildDropdowns(transaction);
                    return View(transaction);
                }


                // Enforce account status logic
                switch (account.Status)
                {
                    case AccountStatus.Closed:
                    case AccountStatus.Frozen:
                        ModelState.AddModelError("", $"Transactions are not allowed on {account.Status} accounts.");
                        RebuildDropdowns(transaction);
                        return View(transaction);

                    case AccountStatus.Inactive:
                        if (transaction.TransactionType != TransactionType.Deposit)
                        {
                            ModelState.AddModelError("", "Only deposits are allowed for inactive accounts.");
                            RebuildDropdowns(transaction);
                            return View(transaction);
                        }
                        break;
                }

                // Adjust balance based on transaction type
                switch (transaction.TransactionType)
                {
                    case TransactionType.Deposit:
                        account.Balance += transaction.Amount;
                        break;

                    case TransactionType.Withdrawal:
                    case TransactionType.Transfer:
                        if (transaction.Amount > account.Balance)
                        {
                            ModelState.AddModelError("Amount", "Insufficient funds for this transaction.");
                            RebuildDropdowns(transaction);
                            return View(transaction);
                        }
                        account.Balance -= transaction.Amount;
                        break;
                }

                _context.Add(transaction);
                _context.Update(account);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Transaction successfully created and balance updated!";
                return RedirectToAction(nameof(Index));
            }

            RebuildDropdowns(transaction);
            return View(transaction);
        }


        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["AccountId"] = new SelectList(
                _context.Accounts.Select(a => new {
                    a.AccountId,
                    a.AccountNumber
                }),
                "AccountId",
                "AccountNumber",
                transaction.AccountId
            );

            ViewData["TransactionType"] = Enum.GetValues(typeof(TransactionType))
                .Cast<TransactionType>()
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = t.ToString(),
                    Selected = t == transaction.TransactionType
                }).ToList();
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,Timestamp,Amount,TransactionType,Description,AccountId")] Transaction transaction)
        {
            // Check if route ID matches the transaction ID
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            // Get the original transaction (no tracking)
            var existingTransaction = await _context.Transactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == id);

            // If transaction doesn't exist
            if (existingTransaction == null)
            {
                return NotFound();
            }

            // If form input is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Keep the original transaction number
                    transaction.TransactionNumber = existingTransaction.TransactionNumber;

                    // Update and save transaction
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If transaction was deleted
                    if (!TransactionExists(transaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Go back to index after successful update
                return RedirectToAction(nameof(Index));
            }

            // If model is invalid, repopulate dropdown and return view
            ViewData["AccountId"] = new SelectList(_context.Accounts, "AccountId", "AccountNumber", transaction.AccountId);
            RebuildDropdowns(transaction);
            return View(transaction);
        }


        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void RebuildDropdowns(Transaction transaction)
        {
            ViewData["AccountId"] = _context.Accounts
                .Select(a => new SelectListItem
                {
                    Value = a.AccountId.ToString(),
                    Text = a.AccountNumber,
                    Selected = a.AccountId == transaction.AccountId
                }).ToList();

            ViewData["TransactionType"] = Enum.GetValues(typeof(TransactionType))
                .Cast<TransactionType>()
                .Select(t => new SelectListItem
                {
                    Text = t.ToString(),
                    Value = t.ToString(),
                    Selected = t == transaction.TransactionType
                }).ToList();
        }

        private string GenerateRandomNumber(int length)
        {
            var random = new Random();
            return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
    }
}
