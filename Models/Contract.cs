namespace FreelancerManagementSystem.Models
{
    public class Contract
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // The legal text
        public decimal TotalAmount { get; set; }
        public DateTime SignedDate { get; set; }
        public DateTime EndDate { get; set; }

        // Status: Draft, Sent, Signed, Terminated
        public string Status { get; set; } = "Draft";

        // Foreign Key to the Freelancer (User)
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Relationship to Project (One contract per project, or vice versa)
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
