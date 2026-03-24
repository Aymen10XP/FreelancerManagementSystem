using FreelancerManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FreelancerManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Your Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<ProjectTask> ProjectTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Validations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            });

            // Configure Relationships (One to many)
            // A Project belongs to one User (Freelancer), but a User has many Projects
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Invoice>()
                            .HasOne(i => i.Contract)
                            .WithMany(c => c.Invoices)
                            .HasForeignKey(i => i.ContractId)
                            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany(u => u.Invoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Freelancer)
                .WithMany() // No navigation property for freelancer invoices collection
                .HasForeignKey(i => i.FreelancerId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
               .HasOne(p => p.Invoice)
               .WithMany(i => i.Payments)
               .HasForeignKey(p => p.InvoiceId)
               .OnDelete(DeleteBehavior.Restrict);

            // Precise decimals for the Contract
            modelBuilder.Entity<Contract>()
                .Property(c => c.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Project)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Freelancer)
                .WithMany(u => u.Contracts)
                .HasForeignKey(c => c.FreelancerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany() // No navigation property for client contracts collection
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure an Invoice can have multiple Payments (Handles partial payments)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(kt => kt.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(kt => kt.ProjectId);

            modelBuilder.Entity<ProjectTask>()
                .HasOne(kt => kt.AssignedTo)
                .WithMany()
                .HasForeignKey(kt => kt.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}


