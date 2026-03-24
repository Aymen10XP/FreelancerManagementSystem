using FreelancerManagementSystem.Data;
using FreelancerManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
        {
            return await _context.Invoices
                .Include(i => i.Contract)
                .Include(i => i.Client)
                .Include(i => i.Freelancer)
                .Include(i => i.Payments)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Contract)
                .Include(i => i.Client)
                .Include(i => i.Freelancer)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return invoice;
        }

        [HttpPost]
        public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
        {
            invoice.Id = Guid.NewGuid();
            invoice.InvoiceNumber = GenerateInvoiceNumber();
            invoice.CreatedAt = DateTime.UtcNow;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, invoice);
        }

        [HttpPost("{id}/payments")]
        public async Task<ActionResult<Payment>> AddPayment(Guid id, Payment payment)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            payment.Id = Guid.NewGuid();
            payment.InvoiceId = id;
            payment.PaymentDate = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Update invoice status if fully paid
            var totalPaid = await _context.Payments
                .Where(p => p.InvoiceId == id && p.Status == "Completed")
                .SumAsync(p => p.Amount);

            if (totalPaid >= invoice.Amount)
            {
                invoice.Status = "Paid";
                invoice.PaidDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        [HttpGet("payments/{id}")]
        public async Task<ActionResult<Payment>> GetPayment(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return payment;
        }

        private string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}
