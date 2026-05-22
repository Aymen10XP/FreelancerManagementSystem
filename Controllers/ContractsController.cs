using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Controllers
{
    /// <summary>
    /// MVC Controller for Web Views - Handles /Contracts routes
    /// </summary>
    public class ContractsController : Controller
    {
        private readonly AppDbContext _context;

        public ContractsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Contracts
        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Project)
                .Include(c => c.Freelancer)
                .Include(c => c.Client)
                .ToListAsync();
            return View(contracts);
        }

        // GET: /Contracts/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View();
        }

        // POST: /Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,FreelancerId,ClientId,Rate,RateType,Title,Content,TotalAmount,StartDate,EndDate,Status,Terms")] Contract contract)
        {
            if (ModelState.IsValid)
            {
                contract.Id = Guid.NewGuid();
                contract.CreatedAt = DateTime.UtcNow;
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(contract);
        }

        // GET: /Contracts/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Project)
                .Include(c => c.Freelancer)
                .Include(c => c.Client)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // GET: /Contracts/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
                return NotFound();

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(contract);
        }

        // POST: /Contracts/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ProjectId,FreelancerId,ClientId,Rate,RateType,Title,Content,TotalAmount,StartDate,EndDate,Status,Terms,CreatedAt")] Contract contract)
        {
            if (id != contract.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    contract.UpdatedAt = DateTime.UtcNow;
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ContractExists(contract.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(contract);
        }

        // GET: /Contracts/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Project)
                .Include(c => c.Freelancer)
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
                return NotFound();

            return View(contract);
        }

        // POST: /Contracts/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ContractExists(Guid id)
        {
            return await _context.Contracts.AnyAsync(e => e.Id == id);
        }
    }
}
