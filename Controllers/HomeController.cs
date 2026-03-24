using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using System.Diagnostics;

namespace FreelancerManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            
            var projects = await _context.Projects.ToListAsync();
            var contracts = await _context.Contracts.ToListAsync();
            var invoices = await _context.Invoices.ToListAsync();

            ViewBag.ProjectCount = projects.Count;
            ViewBag.ContractCount = contracts.Count;
            ViewBag.InvoiceCount = invoices.Count;
            ViewBag.TotalRevenue = invoices.Where(i => i.Status == "Paid").Sum(i => i.Amount);

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
}
