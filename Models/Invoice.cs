namespace FreelancerManagementSystem.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty; // Example: INV-2024-001
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; } = false;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public ICollection<Payment> Payments;
    }
}
