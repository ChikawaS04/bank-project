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

namespace BankApp.Controllers
{
    public class AccountsController : Controller
    {
        private readonly BankDbContext _context;

        public AccountsController(BankDbContext context)
        {
            _context = context;
        }

        // GET: Accounts
        public async Task<IActionResult> Index(string searchString, string statusFilter, int? page, int? pageSizeID)
        {
            // Base query including Client for name-based filtering
            var query = _context.Accounts
                .Include(a => a.Client)
                .AsQueryable();

            // Apply search filter (Account Number or Client Name)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(a =>
                    a.AccountNumber.Contains(searchString) ||
                    a.Client.ClientFirstName.Contains(searchString) ||
                    a.Client.ClientLastName.Contains(searchString));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter) &&
                Enum.TryParse(statusFilter, out AccountStatus status))
            {
                query = query.Where(a => a.Status == status);
            }

            // Setup page size with cookie fallback
            var pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, nameof(Account));
            ViewData["pageSizeID"] = pageSize;
            ViewData["PageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            // Maintain filter state in view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["StatusFilter"] = new SelectList(Enum.GetValues(typeof(AccountStatus)).Cast<AccountStatus>());

            // Paginate
            var paginatedList = await PaginatedList<Account>.CreateAsync(
                query.OrderBy(a => a.AccountNumber),
                page ?? 1,
                pageSize
            );

            return View(paginatedList);
        }


        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id, int page = 1, int? pageSizeID = null)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the account and related client info
            var account = await _context.Accounts
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AccountId == id);

            if (account == null)
            {
                return NotFound();
            }

            // Pagination setup
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, nameof(AccountsController));
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            // Get transactions for the account ordered by most recent
            var transactionsQuery = _context.Transactions
                .Where(t => t.AccountId == id)
                .OrderByDescending(t => t.Timestamp);

            // Apply pagination to the transaction list
            var paginatedTransactions = await PaginatedList<Transaction>.CreateAsync(transactionsQuery, page, pageSize);

            // Pass account info to the view
            ViewData["Account"] = account;

            // Return the paginated transaction list to the Details view
            return View("Details", paginatedTransactions);
        }


        // GET: Accounts/Create
        public IActionResult Create(int? clientId)
        {
            // Client Dropdown
            ViewData["ClientId"] = _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.ClientFirstName + " " + c.ClientLastName
                }).ToList();

            // Account Type Dropdown
            ViewData["AccountType"] = Enum.GetValues(typeof(AccountType))
                .Cast<AccountType>()
                .Select(a => new SelectListItem
                {
                    Text = a.ToString(),
                    Value = a.ToString()
                }).ToList();

            // Status Dropdown
            ViewData["Status"] = Enum.GetValues(typeof(AccountStatus))
                .Cast<AccountStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = s.ToString()
                }).ToList();

            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,AccountType,Balance,OpenDate,Status,ClientId")] Account account)
        {
            // Here for testing. Not important...
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"[MODEL ERROR] {entry.Key}: {error.ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {

                // Generate a unique 10-digit Account Number
                string newAccountNumber;
                do
                {
                    newAccountNumber = GenerateRandomAccountNumber();
                }
                while (_context.Accounts.Any(a => a.AccountNumber == newAccountNumber));

                account.AccountNumber = newAccountNumber;

                _context.Add(account);
                await _context.SaveChangesAsync();

                TempData["Message"] = $" Account created successfully! Account Number: {account.AccountNumber}";
                return RedirectToAction(nameof(Index));
            }

            // Rebuild dropdowns after invalid form submission
            ViewData["ClientId"] = _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.ClientFirstName + " " + c.ClientLastName,
                    Selected = c.ClientId == account.ClientId
                }).ToList();

            ViewData["AccountType"] = new SelectList(
                Enum.GetValues(typeof(AccountType))
                    .Cast<AccountType>()
                    .Select(a => new SelectListItem
                    {
                        Text = a.ToString(),
                        Value = a.ToString()
                    }),
                "Value", "Text", account.AccountType
            );

            ViewData["Status"] = new SelectList(
                Enum.GetValues(typeof(AccountStatus))
                    .Cast<AccountStatus>()
                    .Select(s => new SelectListItem
                    {
                        Text = s.ToString(),
                        Value = s.ToString()
                    }),
                "Value", "Text", account.Status
            );

            return View(account);
        }



        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            ViewData["ClientId"] = new SelectList(
                _context.Clients.Select(c => new {
                    c.ClientId,
                    FullName = c.ClientFirstName + " " + c.ClientLastName
                }),
                "ClientId",
                "FullName",
                account.ClientId
            );

            // Add AccountType dropdown
            ViewData["AccountType"] = new SelectList(
                Enum.GetValues(typeof(AccountType))
                    .Cast<AccountType>()
                    .Select(a => new SelectListItem
                    {
                        Text = a.ToString(),
                        Value = a.ToString()
                    }),
                "Value",
                "Text"
            );

            // Add Status dropdown
            ViewData["Status"] = new SelectList(
                Enum.GetValues(typeof(AccountStatus))
                    .Cast<AccountStatus>()
                    .Select(s => new SelectListItem
                    {
                        Text = s.ToString(),
                        Value = s.ToString()
                    }),
                "Value",
                "Text",
                account.Status // preselect the current value
            );

            if (account.Status == AccountStatus.Closed)
            {
                TempData["Error"] = "Closed accounts cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            RebuildDropdowns(account);
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,AccountType,Balance,OpenDate,Status,ClientId")] Account account)
        {
            // Check if route ID matches the account ID
            if (id != account.AccountId)
            {
                return NotFound();
            }

            // Get the existing account without tracking
            var existingAccount = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.AccountId == id);
            if (existingAccount == null)
            {
                return NotFound();
            }

            // Block edits if the account is closed
            if (existingAccount.Status == AccountStatus.Closed)
            {
                TempData["Error"] = "Closed accounts cannot be updated.";
                return RedirectToAction(nameof(Index));
            }

            // If form data is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Keep the original AccountNumber
                    account.AccountNumber = existingAccount.AccountNumber;

                    // Update and save changes
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Account details updated!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If the account no longer exists
                    if (!AccountExists(account.AccountId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If model state is invalid, reload dropdowns and show form again
            RebuildDropdowns(account);
            return View(account);
        }


        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Client)
                .FirstOrDefaultAsync(m => m.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Account succesfully deleted!";
            return RedirectToAction(nameof(Index));
        }
        private void RebuildDropdowns(Account account)
        {
            ViewData["ClientId"] = _context.Clients
                .Select(c => new SelectListItem
                {
                    Value = c.ClientId.ToString(),
                    Text = c.ClientFirstName + " " + c.ClientLastName,
                    Selected = c.ClientId == account.ClientId
                }).ToList();

            ViewData["AccountType"] = Enum.GetValues(typeof(AccountType))
                .Cast<AccountType>()
                .Select(a => new SelectListItem
                {
                    Text = a.ToString(),
                    Value = a.ToString(),
                    Selected = a == account.AccountType
                }).ToList();

            ViewData["Status"] = Enum.GetValues(typeof(AccountStatus))
                .Cast<AccountStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = s.ToString(),
                    Selected = s == account.Status
                }).ToList();
        }
        private string GenerateRandomAccountNumber()
        {
            var random = new Random();
            return random.Next(1000000000, int.MaxValue).ToString().PadLeft(10, '0').Substring(0, 10);
        }
        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.AccountId == id);
        }
    }
}
