using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using FreelancerManagementSystem.DTOs;
using System.Security.Claims;

namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectTaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectTaskController> _logger;

        public ProjectTaskController(AppDbContext context, ILogger<ProjectTaskController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/projecttask/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectTaskDto>>> GetProjectTasks(Guid projectId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var project = await _context.Projects.FindAsync(projectId);

                if (project == null)
                {
                    return NotFound(new { message = "Project not found" });
                }

                // Check if user has access to this project
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var tasks = await _context.ProjectTasks
                    .Include(t => t.AssignedTo)
                    .Where(t => t.ProjectId == projectId)
                    .OrderBy(t => t.Order)
                    .ToListAsync();

                return Ok(tasks.Select(MapTask));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "Error retrieving tasks" });
            }
        }

        // GET: api/projecttask/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTaskDto>> GetTask(Guid id)
        {
            try
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                var userId = GetCurrentUserId();
                var project = task.Project;

                // Check if user has access to this task's project
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(MapTask(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task {TaskId}", id);
                return StatusCode(500, new { message = "Error retrieving task" });
            }
        }

        // POST: api/projecttask
        [HttpPost]
        public async Task<ActionResult<ProjectTaskDto>> CreateTask([FromBody] CreateTaskDto taskDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var project = await _context.Projects.FindAsync(taskDto.ProjectId);

                if (project == null)
                {
                    return BadRequest(new { message = "Project not found" });
                }

                // Only client, assigned freelancer, or admin can create tasks
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var task = new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = taskDto.ProjectId,
                    Title = taskDto.Title,
                    Description = taskDto.Description,
                    Status = taskDto.Status ?? "Todo",
                    Priority = taskDto.Priority,
                    AssignedToId = taskDto.AssignedToId,
                    DueDate = taskDto.DueDate,
                    CreatedAt = DateTime.UtcNow
                };

                // Set order to end of list for the same status
                var maxOrder = await _context.ProjectTasks
                    .Where(t => t.ProjectId == task.ProjectId && t.Status == task.Status)
                    .MaxAsync(t => (int?)t.Order) ?? -1;
                task.Order = maxOrder + 1;

                _context.ProjectTasks.Add(task);
                await _context.SaveChangesAsync();

                // Load assigned user info
                await _context.Entry(task)
                    .Reference(t => t.AssignedTo)
                    .LoadAsync();

                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, MapTask(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { message = "Error creating task" });
            }
        }

        // PUT: api/projecttask/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto taskDto)
        {
            try
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                var userId = GetCurrentUserId();
                var project = task.Project;

                // Check permissions
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                // Update task properties
                task.Title = taskDto.Title ?? task.Title;
                task.Description = taskDto.Description ?? task.Description;
                task.Status = taskDto.Status ?? task.Status;
                task.Priority = taskDto.Priority ?? task.Priority;
                task.AssignedToId = taskDto.AssignedToId ?? task.AssignedToId;
                task.DueDate = taskDto.DueDate ?? task.DueDate;
                task.UpdatedAt = DateTime.UtcNow;

                _context.Entry(task).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskExists(id))
                {
                    return NotFound(new { message = "Task not found" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, new { message = "Error updating task" });
            }
        }

        // PUT: api/projecttask/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateStatusDto statusDto)
        {
            try
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                var userId = GetCurrentUserId();
                var project = task.Project;

                // Check permissions
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var oldStatus = task.Status;
                task.Status = statusDto.Status;
                task.UpdatedAt = DateTime.UtcNow;

                // Reorder tasks if status changed
                if (oldStatus != task.Status)
                {
                    // Get max order for new status
                    var maxOrder = await _context.ProjectTasks
                        .Where(t => t.ProjectId == task.ProjectId && t.Status == task.Status)
                        .MaxAsync(t => (int?)t.Order) ?? -1;
                    task.Order = maxOrder + 1;
                }

                await _context.SaveChangesAsync();

                // Check if all tasks are done to update project status
                var projectTasks = await _context.ProjectTasks
                    .Where(t => t.ProjectId == task.ProjectId)
                    .ToListAsync();

                if (projectTasks.All(t => t.Status == "Done") && projectTasks.Any())
                {
                    project.Status = "Completed";
                    project.EndDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else if (project.Status == "Completed" && projectTasks.Any(t => t.Status != "Done"))
                {
                    project.Status = "Active";
                    project.EndDate = null;
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status {TaskId}", id);
                return StatusCode(500, new { message = "Error updating task status" });
            }
        }

        // PUT: api/projecttask/{id}/reorder
        [HttpPut("{id}/reorder")]
        public async Task<IActionResult> ReorderTask(Guid id, [FromBody] ReorderTaskDto reorderDto)
        {
            try
            {
                var task = await _context.ProjectTasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                var userId = GetCurrentUserId();
                var project = await _context.Projects.FindAsync(task.ProjectId);

                if (project == null)
                {
                    return NotFound(new { message = "Project not found" });
                }

                // Check permissions
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var tasksInColumn = await _context.ProjectTasks
                    .Where(t => t.ProjectId == task.ProjectId && t.Status == task.Status)
                    .OrderBy(t => t.Order)
                    .ToListAsync();

                // Remove task from list
                tasksInColumn.Remove(task);

                // Insert at new position
                if (reorderDto.NewOrder < 0)
                    reorderDto.NewOrder = 0;
                if (reorderDto.NewOrder > tasksInColumn.Count)
                    reorderDto.NewOrder = tasksInColumn.Count;

                tasksInColumn.Insert(reorderDto.NewOrder, task);

                // Update orders
                for (int i = 0; i < tasksInColumn.Count; i++)
                {
                    tasksInColumn[i].Order = i;
                }

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering task {TaskId}", id);
                return StatusCode(500, new { message = "Error reordering task" });
            }
        }

        // DELETE: api/projecttask/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            try
            {
                var task = await _context.ProjectTasks
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (task == null)
                {
                    return NotFound(new { message = "Task not found" });
                }

                var userId = GetCurrentUserId();
                var project = task.Project;

                // Check permissions
                if (project.ClientId != userId && project.FreelancerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _context.ProjectTasks.Remove(task);
                await _context.SaveChangesAsync();

                // Reorder remaining tasks
                var remainingTasks = await _context.ProjectTasks
                    .Where(t => t.ProjectId == task.ProjectId && t.Status == task.Status)
                    .OrderBy(t => t.Order)
                    .ToListAsync();

                for (int i = 0; i < remainingTasks.Count; i++)
                {
                    remainingTasks[i].Order = i;
                }
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { message = "Error deleting task" });
            }
        }

        // GET: api/projecttask/user/tasks
        [HttpGet("user/tasks")]
        public async Task<ActionResult<IEnumerable<ProjectTaskDto>>> GetMyTasks()
        {
            try
            {
                var userId = GetCurrentUserId();

                var tasks = await _context.ProjectTasks
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Project)
                    .Where(t => t.AssignedToId == userId)
                    .OrderBy(t => t.DueDate)
                    .ThenBy(t => t.Priority)
                    .ToListAsync();

                return Ok(tasks.Select(MapTask));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tasks");
                return StatusCode(500, new { message = "Error retrieving tasks" });
            }
        }

        private async Task<bool> TaskExists(Guid id)
        {
            return await _context.ProjectTasks.AnyAsync(e => e.Id == id);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found");
            }
            return Guid.Parse(userIdClaim);
        }

        private static ProjectTaskDto MapTask(ProjectTask task)
        {
            return new ProjectTaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Order = task.Order,
                Priority = task.Priority,
                AssignedToId = task.AssignedToId,
                AssignedToName = task.AssignedTo == null ? null : $"{task.AssignedTo.FirstName} {task.AssignedTo.LastName}",
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }

    // DTOs for ProjectTask
    public class CreateTaskDto
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Status { get; set; }
        public int Priority { get; set; } = 2;
        public Guid? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int? Priority { get; set; }
        public Guid? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class ReorderTaskDto
    {
        public int NewOrder { get; set; }
    }
}
