using BeatBay.DTOs;
using BeatBay.Data;
using BeatBay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatBay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlansController : ControllerBase
    {
        private readonly BeatBayDbContext _context;

        public PlansController(BeatBayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlans()
        {
            var plans = await _context.Plans
                .Include(p => p.Users)
                .Select(p => new PlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    PriceUSD = p.PriceUSD,
                    MaxConnections = p.MaxConnections,
                    UserCount = p.Users.Count
                })
                .ToListAsync();

            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlanDto>> GetPlan(int id)
        {
            var plan = await _context.Plans
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null)
                return NotFound();

            var planDto = new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                PriceUSD = plan.PriceUSD,
                MaxConnections = plan.MaxConnections,
                UserCount = plan.Users.Count
            };

            return Ok(planDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PlanDto>> CreatePlan(CreatePlanDto dto)
        {
            var plan = new Plan
            {
                Name = dto.Name,
                PriceUSD = dto.PriceUSD,
                MaxConnections = dto.MaxConnections
            };

            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var planDto = new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                PriceUSD = plan.PriceUSD,
                MaxConnections = plan.MaxConnections,
                UserCount = 0
            };

            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, planDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePlan(int id, UpdatePlanDto dto)
        {
            var plan = await _context.Plans.FindAsync(id);
            if (plan == null)
                return NotFound();

            if (dto.Name != null) plan.Name = dto.Name;
            if (dto.PriceUSD.HasValue) plan.PriceUSD = dto.PriceUSD.Value;
            if (dto.MaxConnections.HasValue) plan.MaxConnections = dto.MaxConnections.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Plan updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.Plans.FindAsync(id);
            if (plan == null)
                return NotFound();

            var hasUsers = await _context.Users.AnyAsync(u => u.PlanId == id);
            if (hasUsers)
                return BadRequest(new { message = "Cannot delete plan with active users" });

            _context.Plans.Remove(plan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Plan deleted successfully" });
        }
    }
}
