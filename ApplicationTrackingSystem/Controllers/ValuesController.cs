using ApplicationTrackingSystem.Data;
using ApplicationTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationTrackingSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobPostingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobPostingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var jobs = _context.JobPostings.ToList();
            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var job = _context.JobPostings.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost]
        public IActionResult Create(JobPosting job)
        {
            _context.JobPostings.Add(job);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
        }
    }
}
