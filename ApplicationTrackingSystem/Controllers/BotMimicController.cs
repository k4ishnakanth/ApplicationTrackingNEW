using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApplicationTrackingSystem.Data;
using ApplicationTrackingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTrackingSystem.Controllers
{
    [Authorize(Roles = "BotMimic")]
    [ApiController]
    [Route("[controller]")]
    public class BotMimicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BotMimicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all technical role applications waiting for processing
        [HttpGet("technical-applications")]
        public IActionResult GetTechnicalApplications([FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Applications
                    .Include(a => a.User)
                    .Include(a => a.JobPosting)
                    .Where(a => a.JobPosting != null && a.JobPosting.IsTechnical)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.CurrentStatus == status);
                }

                var applications = query.ToList();

                return Ok(new
                {
                    Message = "Technical role applications for Bot processing",
                    TotalApplications = applications.Count,
                    Applications = applications.Select(a => new
                    {
                        ApplicationId = a.Id,
                        ApplicantName = a.User?.Username,
                        JobTitle = a.JobPosting?.Title,
                        CurrentStatus = a.CurrentStatus,
                        AppliedDate = a.AppliedDate
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Automatically process all "Applied" technical applications
        [HttpPost("process-technical-applications")]
        public IActionResult ProcessTechnicalApplications()
        {
            try
            {
                // Get all "Applied" technical applications
                var applicationsToProcess = _context.Applications
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .Where(a => a.CurrentStatus == "Applied" &&
                                a.JobPosting != null &&
                                a.JobPosting.IsTechnical)
                    .ToList();

                if (applicationsToProcess.Count == 0)
                    return Ok(new { Message = "No technical applications to process" });

                int processedCount = 0;

                foreach (var app in applicationsToProcess)
                {
                    // Bot logic: automatically move to "Under Review"
                    var oldStatus = app.CurrentStatus;
                    app.CurrentStatus = "Under Review";

                    var statusUpdate = new ApplicationStatusUpdate
                    {
                        ApplicationId = app.Id,
                        OldStatus = oldStatus,
                        NewStatus = "Under Review",
                        Comment = "Application received and queued for technical review",
                        UpdatedBy = "BotMimic",
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.ApplicationStatusUpdates.Add(statusUpdate);
                    processedCount++;
                }

                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Technical applications processed successfully",
                    ProcessedCount = processedCount,
                    UpdatedStatus = "Under Review"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Bot auto-updates: Move "Under Review" to "Technical Assessment"
        [HttpPost("schedule-technical-assessment")]
        public IActionResult ScheduleTechnicalAssessment()
        {
            try
            {
                var applicationsUnderReview = _context.Applications
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .Where(a => a.CurrentStatus == "Under Review" &&
                                a.JobPosting != null &&
                                a.JobPosting.IsTechnical)
                    .ToList();

                if (applicationsUnderReview.Count == 0)
                    return Ok(new { Message = "No applications ready for technical assessment" });

                int updatedCount = 0;

                foreach (var app in applicationsUnderReview)
                {
                    var oldStatus = app.CurrentStatus;
                    app.CurrentStatus = "Technical Assessment";

                    var statusUpdate = new ApplicationStatusUpdate
                    {
                        ApplicationId = app.Id,
                        OldStatus = oldStatus,
                        NewStatus = "Technical Assessment",
                        Comment = "Candidate scheduled for technical assessment",
                        UpdatedBy = "BotMimic",
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.ApplicationStatusUpdates.Add(statusUpdate);
                    updatedCount++;
                }

                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Technical assessments scheduled",
                    UpdatedCount = updatedCount,
                    UpdatedStatus = "Technical Assessment"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Bot auto-updates: Move "Technical Assessment" to "Interview"
        [HttpPost("move-to-interview")]
        public IActionResult MoveToInterview()
        {
            try
            {
                var applicationsForInterview = _context.Applications
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .Where(a => a.CurrentStatus == "Technical Assessment" &&
                                a.JobPosting != null &&
                                a.JobPosting.IsTechnical)
                    .ToList();

                if (applicationsForInterview.Count == 0)
                    return Ok(new { Message = "No applications ready for interview" });

                int updatedCount = 0;

                foreach (var app in applicationsForInterview)
                {
                    var oldStatus = app.CurrentStatus;
                    app.CurrentStatus = "Interview";

                    var statusUpdate = new ApplicationStatusUpdate
                    {
                        ApplicationId = app.Id,
                        OldStatus = oldStatus,
                        NewStatus = "Interview",
                        Comment = "Candidate has passed technical assessment. Interview scheduled.",
                        UpdatedBy = "BotMimic",
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.ApplicationStatusUpdates.Add(statusUpdate);
                    updatedCount++;
                }

                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Candidates moved to interview stage",
                    UpdatedCount = updatedCount,
                    UpdatedStatus = "Interview"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Bot auto-updates: Move "Interview" to "Offer"
        [HttpPost("generate-offers")]
        public IActionResult GenerateOffers()
        {
            try
            {
                var applicationsForOffer = _context.Applications
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .Where(a => a.CurrentStatus == "Interview" &&
                                a.JobPosting != null &&
                                a.JobPosting.IsTechnical)
                    .ToList();

                if (applicationsForOffer.Count == 0)
                    return Ok(new { Message = "No applications ready for offers" });

                int updatedCount = 0;

                foreach (var app in applicationsForOffer)
                {
                    var oldStatus = app.CurrentStatus;
                    app.CurrentStatus = "Offer";

                    var statusUpdate = new ApplicationStatusUpdate
                    {
                        ApplicationId = app.Id,
                        OldStatus = oldStatus,
                        NewStatus = "Offer",
                        Comment = "Offer letter generated and sent to candidate",
                        UpdatedBy = "BotMimic",
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.ApplicationStatusUpdates.Add(statusUpdate);
                    updatedCount++;
                }

                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Offers generated successfully",
                    UpdatedCount = updatedCount,
                    UpdatedStatus = "Offer"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Manual bot update for specific application (for testing)
        [HttpPut("auto-update/{applicationId}")]
        public IActionResult AutoUpdateApplication(int applicationId, [FromBody] BotUpdateRequest request)
        {
            try
            {
                var application = _context.Applications
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .FirstOrDefault(a => a.Id == applicationId);

                if (application == null)
                    return NotFound(new { message = "Application not found" });

                // Only allow bot updates for technical roles
                if (application.JobPosting?.IsTechnical == false)
                    return BadRequest(new { message = "Bot can only update technical role applications" });

                var oldStatus = application.CurrentStatus;
                application.CurrentStatus = request.NewStatus;

                var statusUpdate = new ApplicationStatusUpdate
                {
                    ApplicationId = applicationId,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    Comment = request.Comment,
                    UpdatedBy = "BotMimic",
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ApplicationStatusUpdates.Add(statusUpdate);
                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Application auto-updated by Bot",
                    ApplicationId = applicationId,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    UpdatedAt = statusUpdate.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Get Bot processing statistics
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            try
            {
                var technicalApps = _context.Applications
                    .Include(a => a.JobPosting)
                    .Where(a => a.JobPosting != null && a.JobPosting.IsTechnical)
                    .ToList();

                var stats = technicalApps
                    .GroupBy(a => a.CurrentStatus)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                return Ok(new
                {
                    Message = "Bot processing statistics",
                    TotalTechnicalApplications = technicalApps.Count,
                    StatusBreakdown = stats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }
    }

    public class BotUpdateRequest
    {
        public string? NewStatus { get; set; }
        public string? Comment { get; set; }
    }
}
