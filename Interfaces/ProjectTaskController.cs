using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Interfaces
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectTaskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectTask>>> GetProjectTasks(Guid projectId)
        {
            return await _context.ProjectTasks
                .Include(t => t.AssignedTo)
                .Where(t => t.ProjectId == projectId)
                .OrderBy(t => t.Order)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ProjectTask>> CreateTask(ProjectTask task)
        {
            task.Id = Guid.NewGuid();
            task.CreatedAt = DateTime.UtcNow;

            // Set order to end of list
            var maxOrder = await _context.ProjectTasks
                .Where(t => t.ProjectId == task.ProjectId && t.Status == task.Status)
                .MaxAsync(t => (int?)t.Order) ?? -1;
            task.Order = maxOrder + 1;

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTask>> GetTask(Guid id)
        {
            var task = await _context.ProjectTasks
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, ProjectTask task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }

            task.UpdatedAt = DateTime.UtcNow;
            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] string status)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = status;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var task = await _context.ProjectTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.ProjectTasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(Guid id)
        {
            return _context.ProjectTasks.Any(e => e.Id == id);
        }
    }
}

