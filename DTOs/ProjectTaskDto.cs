namespace FreelancerManagementSystem.DTOs
{
    public class ProjectTaskDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Order { get; set; }
        public int Priority { get; set; }
        public Guid? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateProjectTaskDto
    {
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Todo";
        public int Priority { get; set; } = 2;
        public Guid? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class UpdateProjectTaskDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public int? Priority { get; set; }
        public Guid? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
    }
}