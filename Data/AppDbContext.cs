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
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.Freelancer)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Precision for Money/Currency
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // Precise decimals for the Contract
            modelBuilder.Entity<Contract>()
                .Property(c => c.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Ensure an Invoice can have multiple Payments (Handles partial payments)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


