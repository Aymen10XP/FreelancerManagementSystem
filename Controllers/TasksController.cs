using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Controllers
{
    /// <summary>
    /// MVC Controller for Web Views - Handles /Tasks routes
    /// </summary>
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Tasks
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .ToListAsync();
            return View(tasks);
        }

        // GET: /Tasks/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View();
        }

        // POST: /Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,Description,Status,Priority,AssignedToId,DueDate")] ProjectTask task)
        {
            if (ModelState.IsValid)
            {
                task.Id = Guid.NewGuid();
                task.CreatedAt = DateTime.UtcNow;
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(task);
        }

        // GET: /Tasks/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

        // GET: /Tasks/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(task);
        }

        // POST: /Tasks/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ProjectId,Title,Description,Status,Order,Priority,AssignedToId,DueDate,CreatedAt")] ProjectTask task)
        {
            if (id != task.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    task.UpdatedAt = DateTime.UtcNow;
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TaskExists(task.Id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(task);
        }

        // GET: /Tasks/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

        // POST: /Tasks/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task != null)
            {
                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TaskExists(Guid id)
        {
            return await _context.ProjectTasks.AnyAsync(e => e.Id == id);
        }
    }
}
