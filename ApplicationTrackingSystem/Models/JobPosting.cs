namespace ApplicationTrackingSystem.Models
{
    public class JobPosting
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public bool IsTechnical { get; set; }

        public ICollection<Application>? Applications { get; set; }
    }
}
