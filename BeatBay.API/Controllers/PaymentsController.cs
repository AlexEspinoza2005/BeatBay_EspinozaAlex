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
using System.Security.Claims;
using System.Threading.Tasks;

namespace BeatBay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly BeatBayDbContext _context;

        public PaymentsController(BeatBayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Plan)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.Name ?? p.User.UserName,
                    PlanId = p.PlanId,
                    PlanName = p.Plan.Name,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (userId != currentUserId && !isAdmin)
                return Forbid();

            var payments = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Plan)
                .Where(p => p.UserId == userId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User.Name ?? p.User.UserName,
                    PlanId = p.PlanId,
                    PlanName = p.Plan.Name,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount
                })
                .ToListAsync();

            return Ok(payments);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentDto dto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (dto.UserId != currentUserId && !isAdmin)
                return Forbid();

            var user = await _context.Users.FindAsync(dto.UserId);
            var plan = await _context.Plans.FindAsync(dto.PlanId);

            if (user == null || plan == null)
                return BadRequest(new { message = "User or Plan not found" });

            var payment = new Payment
            {
                UserId = dto.UserId,
                PlanId = dto.PlanId,
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                Amount = dto.Amount
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var paymentDto = new PaymentDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                UserName = user.Name ?? user.UserName,
                PlanId = payment.PlanId,
                PlanName = plan.Name,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount
            };

            return CreatedAtAction(nameof(GetPayments), new { id = payment.Id }, paymentDto);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, UpdatePaymentStatusDto dto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = dto.Status;

            // Si el pago se completa, actualizar el plan del usuario
            if (dto.Status == PaymentStatus.Completed)
            {
                var user = await _context.Users.FindAsync(payment.UserId);
                if (user != null)
                {
                    user.PlanId = payment.PlanId;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Payment status updated successfully" });
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly BeatBayDbContext _context;

        public StatisticsController(BeatBayDbContext context)
        {
            _context = context;
        }

        [HttpPost("playback")]
        [Authorize]
        public async Task<IActionResult> RecordPlayback(CreatePlaybackStatisticDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var statistic = new PlaybackStatistic(
                dto.EntityType,
                dto.EntityId,
                DateTime.UtcNow,
                dto.DurationPlayedSeconds,
                dto.PlayCount
            );

            if (dto.SongId.HasValue)
                statistic.SongId = dto.SongId.Value;

            if (dto.UserId.HasValue)
                statistic.UserId = dto.UserId.Value;
            else
                statistic.UserId = userId;

            _context.PlaybackStatistics.Add(statistic);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Playback recorded successfully" });
        }

        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StatisticsSummaryDto>> GetStatisticsSummary()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalSongs = await _context.Songs.CountAsync();
            var totalPlaylists = await _context.Playlists.CountAsync();
            var totalPayments = await _context.Payments.CountAsync();
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

            var planStats = await _context.Plans
                .Include(p => p.Users)
                .Include(p => p.Payments)
                .Select(p => new PlanStatsDto
                {
                    PlanId = p.Id,
                    PlanName = p.Name,
                    UserCount = p.Users.Count,
                    Revenue = p.Payments.Where(pay => pay.Status == PaymentStatus.Completed).Sum(pay => pay.Amount)
                })
                .ToListAsync();

            var summary = new StatisticsSummaryDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalSongs = totalSongs,
                TotalPlaylists = totalPlaylists,
                TotalPayments = totalPayments,
                TotalRevenue = totalRevenue,
                PlanStats = planStats
            };

            return Ok(summary);
        }

        [HttpGet("top-songs")]
        public async Task<ActionResult<TopSongsDto>> GetTopSongs([FromQuery] int limit = 10)
        {
            var topSongs = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.PlaybackStatistics)
                .Where(s => s.IsActive)
                .Select(s => new SongStatsDto
                {
                    SongId = s.Id,
                    Title = s.Title,
                    ArtistName = s.Artist.Name ?? s.Artist.UserName,
                    PlayCount = s.PlaybackStatistics.Sum(ps => ps.PlayCount),
                    TotalDurationPlayed = s.PlaybackStatistics.Sum(ps => ps.DurationPlayedSeconds)
                })
                .OrderByDescending(s => s.PlayCount)
                .Take(limit)
                .ToListAsync();

            return Ok(new TopSongsDto { Songs = topSongs });
        }

        [HttpGet("user/{userId}/playback")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlaybackStatisticDto>>> GetUserPlaybackStats(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (userId != currentUserId && !isAdmin)
                return Forbid();

            var stats = await _context.PlaybackStatistics
                .Include(ps => ps.Song)
                .Include(ps => ps.User)
                .Where(ps => ps.UserId == userId)
                .Select(ps => new PlaybackStatisticDto
                {
                    Id = ps.Id,
                    EntityType = ps.EntityType,
                    EntityId = ps.EntityId,
                    SongId = ps.SongId,
                    SongTitle = ps.Song != null ? ps.Song.Title : null,
                    UserId = ps.UserId,
                    UserName = ps.User != null ? ps.User.Name ?? ps.User.UserName : null,
                    PlaybackDate = ps.PlaybackDate,
                    DurationPlayedSeconds = ps.DurationPlayedSeconds,
                    PlayCount = ps.PlayCount
                })
                .OrderByDescending(ps => ps.PlaybackDate)
                .ToListAsync();

            return Ok(stats);
        }
    }
}
