namespace ApplicationTrackingSystem.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int JobPostingId { get; set; }
        public string? CurrentStatus { get; set; }
        public DateTime AppliedDate { get; set; }

        public User? User { get; set; }
        public JobPosting? JobPosting { get; set; }
        public ICollection<ApplicationStatusUpdate>? StatusUpdates { get; set; }
    }
}
