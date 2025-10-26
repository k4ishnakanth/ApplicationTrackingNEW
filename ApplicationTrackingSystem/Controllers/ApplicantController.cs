using Microsoft.EntityFrameworkCore;
using ApplicationTrackingSystem.Data;
using ApplicationTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationTrackingSystem.Controllers
{
    [Authorize(Roles = "Applicant")]
    [ApiController]
    [Route("[controller]")]
    public class ApplicantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all applications for the current user
        [HttpGet("my-applications")]
        public IActionResult GetMyApplications()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userId, out int parsedUserId))
                    return BadRequest(new { message = "Invalid user ID" });

                var applications = _context.Applications
                    .Where(a => a.UserId == parsedUserId)
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .ToList();

                return Ok(new
                {
                    Message = "Your applications",
                    TotalApplications = applications.Count,
                    Applications = applications.Select(a => new
                    {
                        ApplicationId = a.Id,
                        JobTitle = a.JobPosting?.Title,
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

        // Get details of a specific application
        [HttpGet("application/{id}")]
        public IActionResult GetApplicationDetails(int id)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userId, out int parsedUserId))
                    return BadRequest(new { message = "Invalid user ID" });

                var application = _context.Applications
                    .Where(a => a.Id == id && a.UserId == parsedUserId)
                    .Include(a => a.JobPosting)
                    .Include(a => a.StatusUpdates)
                    .FirstOrDefault();

                if (application == null)
                    return NotFound(new { message = "Application not found" });

                return Ok(new
                {
                    ApplicationId = application.Id,
                    JobTitle = application.JobPosting?.Title,
                    CurrentStatus = application.CurrentStatus,
                    AppliedDate = application.AppliedDate,
                    StatusHistory = application.StatusUpdates?.OrderByDescending(s => s.UpdatedAt)
                        .Select(s => new
                        {
                            Status = s.NewStatus,
                            UpdatedAt = s.UpdatedAt,
                            Comment = s.Comment,
                            UpdatedBy = s.UpdatedBy
                        })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Submit a new application
        [HttpPost("apply")]
        public IActionResult ApplyForJob([FromBody] CreateApplicationRequest request)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userId, out int parsedUserId))
                    return BadRequest(new { message = "Invalid user ID" });

                // Check if job posting exists
                var jobPosting = _context.JobPostings.FirstOrDefault(j => j.Id == request.JobPostingId);
                if (jobPosting == null)
                    return NotFound(new { message = "Job posting not found" });

                // Check if user already applied
                var existingApp = _context.Applications
                    .FirstOrDefault(a => a.UserId == parsedUserId && a.JobPostingId == request.JobPostingId);
                if (existingApp != null)
                    return BadRequest(new { message = "You have already applied for this position" });

                // Create new application
                var application = new Models.Application
                {
                    UserId = parsedUserId,
                    JobPostingId = request.JobPostingId,
                    CurrentStatus = "Applied",
                    AppliedDate = DateTime.UtcNow,
                    StatusUpdates = new List<ApplicationStatusUpdate>()
                };

                // Add initial status update
                var initialUpdate = new ApplicationStatusUpdate
                {
                    OldStatus = null,
                    NewStatus = "Applied",
                    Comment = "Application submitted",
                    UpdatedBy = "System",
                    UpdatedAt = DateTime.UtcNow
                };

                application.StatusUpdates.Add(initialUpdate);
                _context.Applications.Add(application);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetApplicationDetails),
                    new { id = application.Id },
                    new
                    {
                        Message = "Application submitted successfully",
                        ApplicationId = application.Id,
                        JobTitle = jobPosting.Title,
                        Status = "Applied"
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }

        // Get dashboard data for applicant
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (!int.TryParse(userId, out int parsedUserId))
                    return BadRequest(new { message = "Invalid user ID" });

                var applications = _context.Applications
                    .Where(a => a.UserId == parsedUserId)
                    .Include(a => a.JobPosting)
                    .ToList();

                var statusCounts = applications
                    .GroupBy(a => a.CurrentStatus)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                return Ok(new
                {
                    Message = "Applicant Dashboard",
                    TotalApplications = applications.Count,
                    StatusBreakdown = statusCounts,
                    RecentApplications = applications
                        .OrderByDescending(a => a.AppliedDate)
                        .Take(5)
                        .Select(a => new
                        {
                            JobTitle = a.JobPosting?.Title,
                            Status = a.CurrentStatus,
                            AppliedDate = a.AppliedDate
                        })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error: {ex.Message}" });
            }
        }
    }

    public class CreateApplicationRequest
    {
        public int JobPostingId { get; set; }
    }
}
