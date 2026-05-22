namespace FreelancerManagementSystem.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public Guid InvoiceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public Invoice Invoice { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
