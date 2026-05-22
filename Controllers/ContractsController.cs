using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.DTOs;
using FreelancerManagementSystem.Interfaces;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPdfService _pdfService;

        public ContractsController(AppDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContractResponseDto>>> GetContracts()
        {
            var contracts = await BaseQuery().OrderByDescending(c => c.CreatedAt).ToListAsync();
            return Ok(contracts.Select(MapContract));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContractResponseDto>> GetContract(Guid id)
        {
            var contract = await BaseQuery().FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return Ok(MapContract(contract));
        }

        [HttpPost]
        public async Task<ActionResult<ContractResponseDto>> CreateContract(ContractRequestDto dto)
        {
            var validation = await ValidateReferences(dto.ProjectId, dto.ClientId, dto.FreelancerId);
            if (validation != null)
            {
                return validation;
            }

            var contract = new Contract
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.ProjectId,
                ClientId = dto.ClientId,
                FreelancerId = dto.FreelancerId,
                Rate = dto.Rate,
                RateType = dto.RateType,
                Title = dto.Title,
                Content = dto.Content,
                TotalAmount = dto.TotalAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                Terms = dto.Terms,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            var created = await BaseQuery().FirstAsync(c => c.Id == contract.Id);
            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, MapContract(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(Guid id, ContractRequestDto dto)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var validation = await ValidateReferences(dto.ProjectId, dto.ClientId, dto.FreelancerId);
            if (validation != null)
            {
                return validation;
            }

            contract.ProjectId = dto.ProjectId;
            contract.ClientId = dto.ClientId;
            contract.FreelancerId = dto.FreelancerId;
            contract.Rate = dto.Rate;
            contract.RateType = dto.RateType;
            contract.Title = dto.Title;
            contract.Content = dto.Content;
            contract.TotalAmount = dto.TotalAmount;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;
            contract.Status = dto.Status;
            contract.Terms = dto.Terms;
            contract.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(Guid id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Invoices)
                .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            foreach (var invoice in contract.Invoices)
            {
                _context.Payments.RemoveRange(invoice.Payments);
            }
            _context.Invoices.RemoveRange(contract.Invoices);
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadContractPdf(Guid id)
        {
            var contract = await BaseQuery().FirstOrDefaultAsync(c => c.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            var pdf = _pdfService.GenerateContractPdf(contract);
            var fileName = $"contract-{SafeFileName(contract.Title)}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        private IQueryable<Contract> BaseQuery()
        {
            return _context.Contracts
                .Include(c => c.Project)
                .Include(c => c.Client)
                .Include(c => c.Freelancer);
        }

        private async Task<BadRequestObjectResult?> ValidateReferences(Guid projectId, Guid clientId, Guid freelancerId)
        {
            if (!await _context.Projects.AnyAsync(p => p.Id == projectId))
            {
                return BadRequest(new { message = "Project not found" });
            }
            if (!await _context.Users.AnyAsync(u => u.Id == clientId))
            {
                return BadRequest(new { message = "Client not found" });
            }
            if (!await _context.Users.AnyAsync(u => u.Id == freelancerId))
            {
                return BadRequest(new { message = "Freelancer not found" });
            }
            return null;
        }

        private static ContractResponseDto MapContract(Contract contract)
        {
            return new ContractResponseDto
            {
                Id = contract.Id,
                ProjectId = contract.ProjectId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                Rate = contract.Rate,
                RateType = contract.RateType,
                Title = contract.Title,
                Content = contract.Content,
                TotalAmount = contract.TotalAmount,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                Terms = contract.Terms,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt,
                ProjectName = contract.Project?.Name ?? string.Empty,
                Client = MapUser(contract.Client),
                Freelancer = MapUser(contract.Freelancer)
            };
        }

        private static UserDto? MapUser(ApplicationUser? user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        private static string SafeFileName(string value)
        {
            var clean = new string(value
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '-')
                .ToArray());

            while (clean.Contains("--"))
            {
                clean = clean.Replace("--", "-");
            }

            return clean.Trim('-');
        }
    }
}
