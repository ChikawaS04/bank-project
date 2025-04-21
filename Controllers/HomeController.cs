using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BankApp.Models;
using BankApp.Enums;
using BankApp.Data;

namespace BankApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BankDbContext _context;

    public HomeController(ILogger<HomeController> logger, BankDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalClients = await _context.Clients.CountAsync();
        var totalAccounts = await _context.Accounts.CountAsync();
        var activeAccounts = await _context.Accounts.CountAsync(a => a.Status == AccountStatus.Active);
        var totalBalance = await _context.Accounts.SumAsync(a => a.Balance);

        ViewData["TotalClients"] = totalClients;
        ViewData["TotalAccounts"] = totalAccounts;
        ViewData["ActiveAccounts"] = activeAccounts;
        ViewData["TotalBalance"] = totalBalance.ToString("C");

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
