namespace FreelancerManagementSystem.Models
{
    public class Payment
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        // e.g., Stripe, PayPal, Bank Transfer
        public string PaymentMethod { get; set; } = string.Empty;

        // Transaction ID from the provider (External Reference)

        public string TransactionId { get; set; } = string.Empty;

        // Link to the Invoice
        public Guid InvoiceId { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Completed, Failed
        public string Notes { get; set; } = string.Empty;
        public Invoice Invoice { get; set; } = null!;

        // Link to the User (The one who paid) 
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
