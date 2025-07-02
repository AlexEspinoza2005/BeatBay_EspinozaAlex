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
    public class SongsController : ControllerBase
    {
        private readonly BeatBayDbContext _context;

        public SongsController(BeatBayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SongDto>>> GetSongs([FromQuery] bool? isActive = null)
        {
            var query = _context.Songs.Include(s => s.Artist).AsQueryable();

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            var songs = await query
                .Select(s => new SongDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Duration = s.Duration,
                    Genre = s.Genre,
                    StreamingUrl = s.StreamingUrl,
                    ArtistId = s.ArtistId,
                    ArtistName = s.Artist.Name ?? s.Artist.UserName,
                    IsActive = s.IsActive,
                    UploadedAt = s.UploadedAt,
                    PlayCount = s.PlaybackStatistics.Sum(ps => ps.PlayCount)
                })
                .ToListAsync();

            return Ok(songs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SongDto>> GetSong(int id)
        {
            var song = await _context.Songs
                .Include(s => s.Artist)
                .Include(s => s.PlaybackStatistics)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (song == null)
                return NotFound();

            var songDto = new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                Duration = song.Duration,
                Genre = song.Genre,
                StreamingUrl = song.StreamingUrl,
                ArtistId = song.ArtistId,
                ArtistName = song.Artist.Name ?? song.Artist.UserName,
                IsActive = song.IsActive,
                UploadedAt = song.UploadedAt,
                PlayCount = song.PlaybackStatistics.Sum(ps => ps.PlayCount)
            };

            return Ok(songDto);
        }

        [HttpPost]
        [Authorize(Roles = "Artist,Admin")]
        public async Task<ActionResult<SongDto>> CreateSong(CreateSongDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var song = new Song
            {
                Title = dto.Title,
                Duration = dto.Duration,
                Genre = dto.Genre,
                StreamingUrl = dto.StreamingUrl,
                ArtistId = userId
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            var artist = await _context.Users.FindAsync(userId);
            var songDto = new SongDto
            {
                Id = song.Id,
                Title = song.Title,
                Duration = song.Duration,
                Genre = song.Genre,
                StreamingUrl = song.StreamingUrl,
                ArtistId = song.ArtistId,
                ArtistName = artist?.Name ?? artist?.UserName,
                IsActive = song.IsActive,
                UploadedAt = song.UploadedAt,
                PlayCount = 0
            };

            return CreatedAtAction(nameof(GetSong), new { id = song.Id }, songDto);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateSong(int id, UpdateSongDto dto)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (song.ArtistId != userId && !isAdmin)
                return Forbid();

            if (dto.Title != null) song.Title = dto.Title;
            if (dto.Duration.HasValue) song.Duration = dto.Duration.Value;
            if (dto.Genre != null) song.Genre = dto.Genre;
            if (dto.StreamingUrl != null) song.StreamingUrl = dto.StreamingUrl;
            if (dto.IsActive.HasValue) song.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Song updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (song.ArtistId != userId && !isAdmin)
                return Forbid();

            song.IsActive = false;
            await _context.SaveChangesAsync();

            if (isAdmin && song.ArtistId != userId)
            {
                var log = new AdminActionLog
                {
                    AdminUserId = userId,
                    ActionType = "Delete Song",
                    Description = $"Deactivated song: {song.Title}"
                };
                _context.AdminActionLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Song deactivated successfully" });
        }
    }
}
