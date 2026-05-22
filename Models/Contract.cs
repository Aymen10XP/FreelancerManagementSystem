namespace FreelancerManagementSystem.Models
{
    public class Contract
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid FreelancerId { get; set; }
        public Guid ClientId { get; set; }
        public decimal Rate { get; set; }
        public string RateType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = "Draft";
        public string Terms { get; set; } = string.Empty;

        public Project Project { get; set; } = null!;
        public ApplicationUser Freelancer { get; set; } = null!;
        public ApplicationUser Client { get; set; } = null!;
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
