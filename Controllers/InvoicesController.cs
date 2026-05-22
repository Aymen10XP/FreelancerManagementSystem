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
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPdfService _pdfService;

        public InvoicesController(AppDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetInvoices()
        {
            var invoices = await BaseQuery().OrderByDescending(i => i.IssueDate).ToListAsync();
            return Ok(invoices.Select(MapInvoice));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceResponseDto>> GetInvoice(Guid id)
        {
            var invoice = await BaseQuery().FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(MapInvoice(invoice));
        }

        [HttpPost]
        public async Task<ActionResult<InvoiceResponseDto>> CreateInvoice(InvoiceRequestDto dto)
        {
            var validation = await ValidateReferences(dto.ContractId, dto.ClientId, dto.FreelancerId);
            if (validation != null)
            {
                return validation;
            }

            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = GenerateInvoiceNumber(),
                Amount = dto.Amount,
                ContractId = dto.ContractId,
                ClientId = dto.ClientId,
                FreelancerId = dto.FreelancerId,
                Status = dto.Status,
                IssueDate = dto.IssueDate,
                DueDate = dto.DueDate,
                PaidDate = dto.PaidDate,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var created = await BaseQuery().FirstAsync(i => i.Id == invoice.Id);
            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, MapInvoice(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(Guid id, InvoiceRequestDto dto)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            var validation = await ValidateReferences(dto.ContractId, dto.ClientId, dto.FreelancerId);
            if (validation != null)
            {
                return validation;
            }

            invoice.Amount = dto.Amount;
            invoice.ContractId = dto.ContractId;
            invoice.ClientId = dto.ClientId;
            invoice.FreelancerId = dto.FreelancerId;
            invoice.Status = dto.Status;
            invoice.IssueDate = dto.IssueDate;
            invoice.DueDate = dto.DueDate;
            invoice.PaidDate = dto.PaidDate;
            invoice.Description = dto.Description;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await UpdateInvoicePaymentStatus(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            _context.Payments.RemoveRange(invoice.Payments);
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadInvoicePdf(Guid id)
        {
            var invoice = await BaseQuery().FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            var pdf = _pdfService.GenerateInvoicePdf(invoice);
            var fileName = $"invoice-{SafeFileName(invoice.InvoiceNumber)}.pdf";
            return File(pdf, "application/pdf", fileName);
        }

        [HttpGet("{id}/payments")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetInvoicePayments(Guid id)
        {
            if (!await _context.Invoices.AnyAsync(i => i.Id == id))
            {
                return NotFound();
            }

            var payments = await _context.Payments
                .Include(p => p.User)
                .Where(p => p.InvoiceId == id)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return Ok(payments.Select(MapPayment));
        }

        [HttpPost("{id}/payments")]
        public async Task<ActionResult<PaymentResponseDto>> AddPayment(Guid id, PaymentRequestDto dto)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                return BadRequest(new { message = "Payment user not found" });
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = id,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                TransactionId = dto.TransactionId,
                Status = dto.Status,
                Notes = dto.Notes,
                UserId = dto.UserId,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            await UpdateInvoicePaymentStatus(id);

            var created = await _context.Payments.Include(p => p.User).FirstAsync(p => p.Id == payment.Id);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, MapPayment(created));
        }

        [HttpGet("payments/{id}")]
        public async Task<ActionResult<PaymentResponseDto>> GetPayment(Guid id)
        {
            var payment = await _context.Payments.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null)
            {
                return NotFound();
            }
            return Ok(MapPayment(payment));
        }

        [HttpPut("payments/{id}")]
        public async Task<IActionResult> UpdatePayment(Guid id, PaymentRequestDto dto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
            {
                return BadRequest(new { message = "Payment user not found" });
            }

            payment.Amount = dto.Amount;
            payment.PaymentMethod = dto.PaymentMethod;
            payment.TransactionId = dto.TransactionId;
            payment.Status = dto.Status;
            payment.Notes = dto.Notes;
            payment.UserId = dto.UserId;

            await _context.SaveChangesAsync();
            await UpdateInvoicePaymentStatus(payment.InvoiceId);
            return NoContent();
        }

        [HttpDelete("payments/{id}")]
        public async Task<IActionResult> DeletePayment(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            var invoiceId = payment.InvoiceId;
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            await UpdateInvoicePaymentStatus(invoiceId);

            return NoContent();
        }

        private IQueryable<Invoice> BaseQuery()
        {
            return _context.Invoices
                .Include(i => i.Contract)
                .ThenInclude(c => c.Project)
                .Include(i => i.Client)
                .Include(i => i.Freelancer)
                .Include(i => i.Payments)
                .ThenInclude(p => p.User);
        }

        private async Task<BadRequestObjectResult?> ValidateReferences(Guid contractId, Guid clientId, Guid freelancerId)
        {
            if (!await _context.Contracts.AnyAsync(c => c.Id == contractId))
            {
                return BadRequest(new { message = "Contract not found" });
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

        private async Task UpdateInvoicePaymentStatus(Guid invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                return;
            }

            var totalPaid = invoice.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount);
            if (totalPaid >= invoice.Amount && invoice.Amount > 0)
            {
                invoice.Status = "Paid";
                invoice.PaidDate ??= DateTime.UtcNow;
            }
            else if (totalPaid > 0)
            {
                invoice.Status = "Partially Paid";
                invoice.PaidDate = null;
            }
            else if (invoice.Status == "Paid" || invoice.Status == "Partially Paid")
            {
                invoice.Status = "Sent";
                invoice.PaidDate = null;
            }

            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static InvoiceResponseDto MapInvoice(Invoice invoice)
        {
            return new InvoiceResponseDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Amount = invoice.Amount,
                ContractId = invoice.ContractId,
                ClientId = invoice.ClientId,
                FreelancerId = invoice.FreelancerId,
                Status = invoice.Status,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                PaidDate = invoice.PaidDate,
                Description = invoice.Description,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                ContractTitle = invoice.Contract?.Title ?? string.Empty,
                Client = MapUser(invoice.Client),
                Freelancer = MapUser(invoice.Freelancer),
                TotalPaid = invoice.Payments.Where(p => p.Status == "Completed").Sum(p => p.Amount),
                Payments = invoice.Payments.OrderByDescending(p => p.PaymentDate).Select(MapPayment).ToList()
            };
        }

        private static PaymentResponseDto MapPayment(Payment payment)
        {
            return new PaymentResponseDto
            {
                Id = payment.Id,
                InvoiceId = payment.InvoiceId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                TransactionId = payment.TransactionId,
                Status = payment.Status,
                Notes = payment.Notes,
                UserId = payment.UserId,
                UserName = payment.User == null ? string.Empty : $"{payment.User.FirstName} {payment.User.LastName}"
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

        private static string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
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
