using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApplicationTrackingSystem.Data;
using ApplicationTrackingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ApplicationTrackingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all applications (for admin to manage)
        // Get all applications (for admin to manage)
        [HttpGet("all-applications")]
        public IActionResult GetAllApplications([FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Applications
                    .Include(a => a.User)
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.CurrentStatus == status);
                }

                var applications = query.ToList();

                return Ok(new
                {
                    Message = "All applications",
                    TotalApplications = applications.Count,
                    Applications = applications.Select(a => new
                    {
                        ApplicationId = a.Id,
                        ApplicantName = a.User?.Username,
                        JobTitle = a.JobPosting?.Title,
                        IsTechnicalRole = a.JobPosting?.IsTechnical,
                        CurrentStatus = a.CurrentStatus,
                        AppliedDate = a.AppliedDate,
                        UpdatesCount = a.StatusUpdates?.Count ?? 0
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }


        // Get non-technical applications only (for manual management)
        [HttpGet("non-technical-applications")]
        public IActionResult GetNonTechnicalApplications()
        {
            try
            {
                var applications = _context.Applications
                    .Include(a => a.User)
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .Where(a => a.JobPosting != null && !a.JobPosting.IsTechnical)
                    .ToList();

                return Ok(new
                {
                    Message = "Non-technical role applications (manual management)",
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

        // Get application details
        [HttpGet("application/{id}")]
        public IActionResult GetApplicationDetails(int id)
        {
            try
            {
                var application = _context.Applications
                    .Include(a => a.User)
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .FirstOrDefault(a => a.Id == id);

                if (application == null)
                    return NotFound(new { message = "Application not found" });

                return Ok(new
                {
                    ApplicationId = application.Id,
                    ApplicantName = application.User?.Username,
                    JobTitle = application.JobPosting?.Title,
                    IsTechnicalRole = application.JobPosting?.IsTechnical,
                    CurrentStatus = application.CurrentStatus,
                    AppliedDate = application.AppliedDate,
                    StatusHistory = application.StatusUpdates?
                        .OrderByDescending(s => s.UpdatedAt)
                        .Select(s => new
                        {
                            OldStatus = s.OldStatus,
                            NewStatus = s.NewStatus,
                            Comment = s.Comment,
                            UpdatedBy = s.UpdatedBy,
                            UpdatedAt = s.UpdatedAt
                        })
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Manually update application status (for non-technical roles)
        [HttpPut("update-application-status/{id}")]
        public IActionResult UpdateApplicationStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var application = _context.Applications
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .FirstOrDefault(a => a.Id == id);

                if (application == null)
                    return NotFound(new { message = "Application not found" });

                // Only allow manual updates for non-technical roles
                if (application.JobPosting?.IsTechnical == true)
                    return BadRequest(new { message = "Technical role applications are auto-managed by Bot Mimic" });

                var oldStatus = application.CurrentStatus;
                application.CurrentStatus = request.NewStatus;

                // Create status update record
                var statusUpdate = new ApplicationStatusUpdate
                {
                    ApplicationId = id,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    Comment = request.Comment,
                    UpdatedBy = "Admin",
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ApplicationStatusUpdates.Add(statusUpdate);
                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Application status updated successfully",
                    ApplicationId = id,
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

        // Create a new job posting
        [HttpPost("create-job-posting")]
        public IActionResult CreateJobPosting([FromBody] CreateJobPostingRequest request)
        {
            try
            {
                var jobPosting = new JobPosting
                {
                    Title = request.Title,
                    IsTechnical = request.IsTechnical
                };

                _context.JobPostings.Add(jobPosting);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetApplicationDetails),
                    new { id = jobPosting.Id },
                    new
                    {
                        Message = "Job posting created successfully",
                        JobPostingId = jobPosting.Id,
                        Title = jobPosting.Title,
                        IsTechnical = jobPosting.IsTechnical
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Get all job postings
        [HttpGet("job-postings")]
        public IActionResult GetJobPostings()
        {
            try
            {
                var jobPostings = _context.JobPostings.ToList();

                return Ok(new
                {
                    Message = "All job postings",
                    TotalPostings = jobPostings.Count,
                    JobPostings = jobPostings.Select(j => new
                    {
                        JobPostingId = j.Id,
                        Title = j.Title,
                        IsTechnical = j.IsTechnical
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Admin Dashboard
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            try
            {
                var allApplications = _context.Applications
                    .Include(a => a.JobPosting)
                    .ToList();

                var technicalApps = allApplications.Where(a => a.JobPosting?.IsTechnical == true).ToList();
                var nonTechnicalApps = allApplications.Where(a => a.JobPosting?.IsTechnical == false).ToList();

                var statusCounts = allApplications
                    .GroupBy(a => a.CurrentStatus)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                return Ok(new
                {
                    Message = "Admin Dashboard",
                    TotalApplications = allApplications.Count,
                    TechnicalApplications = technicalApps.Count,
                    NonTechnicalApplications = nonTechnicalApps.Count,
                    StatusBreakdown = statusCounts,
                    JobPostingsCount = _context.JobPostings.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string? NewStatus { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateJobPostingRequest
    {
        public string? Title { get; set; }
        public bool IsTechnical { get; set; }
    }
}
