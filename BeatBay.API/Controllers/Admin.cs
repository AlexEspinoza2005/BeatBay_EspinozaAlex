using BeatBay.API.DTOs;
using BeatBay.Data;
using BeatBay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BeatBay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly BeatBayDbContext _context;

        public AdminController(BeatBayDbContext context)
        {
            _context = context;
        }

        [HttpGet("action-logs")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<AdminActionLogDto>>> GetActionLogs()
        {
            var logs = await _context.AdminActionLogs
                .Include(aal => aal.AdminUser)
                .Select(aal => new AdminActionLogDto
                {
                    Id = aal.Id,
                    AdminUserId = aal.AdminUserId,
                    AdminUserName = aal.AdminUser.Name ?? aal.AdminUser.UserName,
                    ActionType = aal.ActionType,
                    Description = aal.Description,
                    ActionDate = aal.ActionDate
                })
                .OrderByDescending(aal => aal.ActionDate)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpPost("action-logs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateActionLog(CreateAdminActionLogDto dto)
        {
            var adminUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var log = new AdminActionLog
            {
                AdminUserId = adminUserId,
                ActionType = dto.ActionType,
                Description = dto.Description
            };

            _context.AdminActionLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Action logged successfully" });
        }
    }
}
