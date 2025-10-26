using Microsoft.EntityFrameworkCore;
using ApplicationTrackingSystem.Models;

namespace ApplicationTrackingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Use fully qualified name
        public DbSet<Models.Application> Applications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<ApplicationStatusUpdate> ApplicationStatusUpdates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "applicant1", Password = "password1", Role = "Applicant" },
                new User { Id = 2, Username = "botmimic", Password = "password2", Role = "BotMimic" },
                new User { Id = 3, Username = "admin", Password = "password3", Role = "Admin" }
            );

            modelBuilder.Entity<JobPosting>().HasData(
                new JobPosting { Id = 1, Title = "Senior Software Engineer", IsTechnical = true },
                new JobPosting { Id = 2, Title = "Junior Developer", IsTechnical = true },
                new JobPosting { Id = 3, Title = "HR Executive", IsTechnical = false }
            );
        }
    }
}
