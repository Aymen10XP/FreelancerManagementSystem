using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerManagementSystem.Data;

namespace FreelancerManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard/{userId}")]
        public async Task<ActionResult<object>> GetDashboardAnalytics(Guid userId)
        {
            var projects = await _context.Projects
                .Where(p => p.FreelancerId == userId || p.ClientId == userId)
                .ToListAsync();

            var invoices = await _context.Invoices
                .Where(i => i.FreelancerId == userId || i.ClientId == userId)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => _context.Invoices
                    .Where(i => i.FreelancerId == userId || i.ClientId == userId)
                    .Select(i => i.Id)
                    .Contains(p.InvoiceId))
                .ToListAsync();

            var analytics = new
            {
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.Status == "Active"),
                CompletedProjects = projects.Count(p => p.Status == "Completed"),
                TotalInvoiced = invoices.Sum(i => i.Amount),
                TotalPaid = payments.Where(p => p.Status == "Completed").Sum(p => p.Amount),
                OutstandingInvoices = invoices.Where(i => i.Status != "Paid").Sum(i => i.Amount),
                ProjectsByStatus = projects.GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() }),
                InvoicesByMonth = invoices.GroupBy(i => i.IssueDate.ToString("yyyy-MM"))
                    .Select(g => new { Month = g.Key, Total = g.Sum(i => i.Amount) })
                    .OrderBy(g => g.Month)
            };

            return Ok(analytics);
        }

        [HttpGet("freelancer-performance/{freelancerId}")]
        public async Task<ActionResult<object>> GetFreelancerPerformance(Guid freelancerId)
        {
            var projects = await _context.Projects
                .Where(p => p.FreelancerId == freelancerId)
                .Include(p => p.Tasks)
                .ToListAsync();

            var invoices = await _context.Invoices
                .Where(i => i.FreelancerId == freelancerId)
                .ToListAsync();

            var performance = new
            {
                TotalProjects = projects.Count,
                CompletedProjects = projects.Count(p => p.Status == "Completed"),
                AverageProjectDuration = projects
                    .Where(p => p.EndDate.HasValue)
                    .Average(p => (p.EndDate.Value - p.StartDate).TotalDays),
                TotalEarnings = invoices.Where(i => i.Status == "Paid").Sum(i => i.Amount),
                PendingEarnings = invoices.Where(i => i.Status != "Paid").Sum(i => i.Amount),
                TaskCompletionRate = projects
                    .SelectMany(p => p.Tasks)
                    .Count(t => t.Status == "Done") /
                    (double)Math.Max(1, projects.SelectMany(p => p.Tasks).Count()) * 100,
                ProjectsByMonth = projects.GroupBy(p => p.StartDate.ToString("yyyy-MM"))
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Month)
            };

            return Ok(performance);
        }
    }
}
