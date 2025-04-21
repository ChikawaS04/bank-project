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

namespace BankApp.Controllers
{
    public class ClientsController : Controller
    {
        private readonly BankDbContext _context;

        public ClientsController(BankDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string searchString, string cityFilter, int? page, int? pageSizeID)
        {

            // Fetch city list for dropdown filter
            var cityList = await _context.Clients
                .Select(c => c.ClientCityAddress)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            ViewBag.CityList = cityList;

            // Build the query
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(c =>
                    (c.ClientFirstName + " " + c.ClientLastName).Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(cityFilter))
            {
                query = query.Where(c => c.ClientCityAddress == cityFilter);
            }

            // Setup paging
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCity"] = cityFilter;

            var pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, nameof(Client));
            ViewData["pageSizeID"] = pageSize;
            ViewData["PageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var paginated = await PaginatedList<Client>.CreateAsync(query, page ?? 1, pageSize);
            return View(paginated);
        }



        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id, int? pageSizeID, int? page)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                //.Include(c => c.Accounts)
                .FirstOrDefaultAsync(m => m.ClientId == id);

            if (client == null)
            {
                return NotFound();
            }

            // Pagination setup
            var pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "ClientDetails");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);

            var accounts = _context.Accounts
                .Where(a => a.ClientId == id)
                .OrderByDescending(a => a.OpenDate)
                .Include(a => a.Client);

            var paginatedAccounts = await PaginatedList<Account>.CreateAsync(accounts.AsNoTracking(), page ?? 1, pageSize);

            ViewData["Client"] = client;
            return View(paginatedAccounts);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,ClientFirstName,ClientMiddleName,ClientLastName,ClientPhone,ClientEmail,ClientStreetAddress,ClientPostalCode,ClientCityAddress,ClientCountryAddress")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Client created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,ClientFirstName,ClientMiddleName,ClientLastName,ClientPhone,ClientEmail,ClientStreetAddress,ClientPostalCode,ClientCityAddress,ClientCountryAddress")] Client client)
        {
            // Check if route ID matches the client's ID
            if (id != client.ClientId)
            {
                return NotFound();
            }

            // If form data is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update client in database
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If client no longer exists
                    if (!ClientExists(client.ClientId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Show success message and go back to index
                TempData["Message"] = "Client details updated!";
                return RedirectToAction(nameof(Index));
            }

            // If invalid, return form with errors
            return View(client);
        }


        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Client succesfully deleted!";
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }
    }
}
