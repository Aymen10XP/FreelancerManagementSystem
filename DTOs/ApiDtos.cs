namespace FreelancerManagementSystem.DTOs
{
    public class ContractRequestDto
    {
        public Guid ProjectId { get; set; }
        public Guid FreelancerId { get; set; }
        public Guid ClientId { get; set; }
        public decimal Rate { get; set; }
        public string RateType { get; set; } = "Fixed";
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string Terms { get; set; } = string.Empty;
    }

    public class ContractResponseDto : ContractRequestDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public UserDto? Client { get; set; }
        public UserDto? Freelancer { get; set; }
    }

    public class InvoiceRequestDto
    {
        public decimal Amount { get; set; }
        public Guid ContractId { get; set; }
        public Guid ClientId { get; set; }
        public Guid FreelancerId { get; set; }
        public string Status { get; set; } = "Sent";
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class InvoiceResponseDto : InvoiceRequestDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ContractTitle { get; set; } = string.Empty;
        public UserDto? Client { get; set; }
        public UserDto? Freelancer { get; set; }
        public decimal TotalPaid { get; set; }
        public List<PaymentResponseDto> Payments { get; set; } = new();
    }

    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Bank transfer";
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public string Notes { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }

    public class PaymentResponseDto : PaymentRequestDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class UserResponseDto : UserDto
    {
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
