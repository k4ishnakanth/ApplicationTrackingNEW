namespace ApplicationTrackingSystem.Models
{
    public class ApplicationStatusUpdate
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Comment { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Application? Application { get; set; }
    }
}
