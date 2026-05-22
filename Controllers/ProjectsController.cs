using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using FreelancerManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Controllers
{
    /// <summary>
    /// MVC Controller for Web Views - Handles /Projects routes
    /// </summary>
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Projects
        public async Task<IActionResult> Index(string? status = null, string? sortBy = "CreatedAt")
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Freelancer)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Sort by the specified column
            query = sortBy?.ToLower() switch
            {
                "name" => query.OrderBy(p => p.Name),
                "budget" => query.OrderByDescending(p => p.Budget),
                "startdate" => query.OrderByDescending(p => p.StartDate),
                "status" => query.OrderBy(p => p.Status),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var projects = await query.ToListAsync();
            return View(projects);
        }

        // GET: /Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,Budget,Status,ClientId,FreelancerId")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Id = Guid.NewGuid();
                project.CreatedAt = DateTime.UtcNow;
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: /Projects/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Freelancer)
                .Include(p => p.Contracts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
                return NotFound();

            return View(project);
        }

        // GET: /Projects/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: /Projects/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Description,StartDate,EndDate,Budget,Status,ClientId,FreelancerId")] Project project)
        {
            if (id != project.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    project.UpdatedAt = DateTime.UtcNow;
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: /Projects/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
                return NotFound();

            return View(project);
        }

        // POST: /Projects/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(Guid id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
