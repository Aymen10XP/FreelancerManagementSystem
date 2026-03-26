namespace FreelancerManagementSystem.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty; // Example: INV-2024-001
        public decimal Amount { get; set; }
        public Guid ContractId { get; set; }
        public Guid ClientId { get; set; }
        public Guid FreelancerId { get; set; }
        public string Status { get; set; } = string.Empty; // Draft, Sent, Paid, Overdue

        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Contract Contract { get; set; } = null!;
        public User Client { get; set; } = null!;
        public User Freelancer { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
