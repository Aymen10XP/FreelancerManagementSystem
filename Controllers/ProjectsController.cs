using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerManagementSystem.DTOs;


namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }




        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Freelancer)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Budget = p.Budget,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Client = p.Client != null ? new UserDto
                    {
                        Id = p.Client.Id,
                        Email = p.Client.Email,
                        FirstName = p.Client.FirstName,
                        LastName = p.Client.LastName,
                        Role = p.Client.Role
                    } : null,
                    Freelancer = p.Freelancer != null ? new UserDto
                    {
                        Id = p.Freelancer.Id,
                        Email = p.Freelancer.Email,
                        FirstName = p.Freelancer.FirstName,
                        LastName = p.Freelancer.LastName,
                        Role = p.Freelancer.Role
                    } : null
                })
                .ToListAsync();

            return Ok(projects);
        }

        // GET: api/projects/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponseDto>> GetProject(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Freelancer)
                .Include(p => p.ProjectTasks)
                .Where(p => p.Id == id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Budget = p.Budget,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Client = p.Client != null ? new UserDto
                    {
                        Id = p.Client.Id,
                        Email = p.Client.Email,
                        FirstName = p.Client.FirstName,
                        LastName = p.Client.LastName,
                        Role = p.Client.Role
                    } : null,
                    Freelancer = p.Freelancer != null ? new UserDto
                    {
                        Id = p.Freelancer.Id,
                        Email = p.Freelancer.Email,
                        FirstName = p.Freelancer.FirstName,
                        LastName = p.Freelancer.LastName,
                        Role = p.Freelancer.Role
                    } : null
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectResponseDto>> CreateProject(CreateProjectDto createDto)
        {
            // Validate Client exists
            var client = await _context.Users.FindAsync(createDto.ClientId);
            if (client == null)
            {
                return BadRequest($"Client with ID {createDto.ClientId} not found");
            }

            // Validate Freelancer if provided
            if (createDto.FreelancerId.HasValue)
            {
                var freelancer = await _context.Users.FindAsync(createDto.FreelancerId.Value);
                if (freelancer == null)
                {
                    return BadRequest($"Freelancer with ID {createDto.FreelancerId} not found");
                }
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                Status = createDto.Status,
                StartDate = createDto.StartDate,
                Budget = createDto.Budget,
                ClientId = createDto.ClientId,
                FreelancerId = createDto.FreelancerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Load the created project with related data
            var createdProject = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Freelancer)
                .Where(p => p.Id == project.Id)
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Budget = p.Budget,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Client = p.Client != null ? new UserDto
                    {
                        Id = p.Client.Id,
                        Email = p.Client.Email,
                        FirstName = p.Client.FirstName,
                        LastName = p.Client.LastName,
                        Role = p.Client.Role
                    } : null,
                    Freelancer = p.Freelancer != null ? new UserDto
                    {
                        Id = p.Freelancer.Id,
                        Email = p.Freelancer.Email,
                        FirstName = p.Freelancer.FirstName,
                        LastName = p.Freelancer.LastName,
                        Role = p.Freelancer.Role
                    } : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, createdProject);
        }

        // PUT: api/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, UpdateProjectDto updateDto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Validate Client exists
            var client = await _context.Users.FindAsync(updateDto.ClientId);
            if (client == null)
            {
                return BadRequest($"Client with ID {updateDto.ClientId} not found");
            }

            // Validate Freelancer if provided
            if (updateDto.FreelancerId.HasValue)
            {
                var freelancer = await _context.Users.FindAsync(updateDto.FreelancerId.Value);
                if (freelancer == null)
                {
                    return BadRequest($"Freelancer with ID {updateDto.FreelancerId} not found");
                }
            }

            project.Name = updateDto.Name;
            project.Description = updateDto.Description;
            project.Status = updateDto.Status;
            project.StartDate = updateDto.StartDate;
            project.EndDate = updateDto.EndDate;
            project.Budget = updateDto.Budget;
            project.ClientId = updateDto.ClientId;
            project.FreelancerId = updateDto.FreelancerId;
            project.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/projects/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(Guid id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
