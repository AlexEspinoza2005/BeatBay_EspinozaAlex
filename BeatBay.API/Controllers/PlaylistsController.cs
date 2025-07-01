using BeatBay.API.DTOs;
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
    public class PlaylistsController : ControllerBase
    {
        private readonly BeatBayDbContext _context;

        public PlaylistsController(BeatBayDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PlaylistDto>>> GetPlaylists()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .ThenInclude(s => s.Artist)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(p => p.UserId == userId);

            var playlists = await query
                .Select(p => new PlaylistDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    UserId = p.UserId,
                    UserName = p.User.Name ?? p.User.UserName,
                    Songs = p.PlaylistSongs.Select(ps => new SongDto
                    {
                        Id = ps.Song.Id,
                        Title = ps.Song.Title,
                        Duration = ps.Song.Duration,
                        Genre = ps.Song.Genre,
                        StreamingUrl = ps.Song.StreamingUrl,
                        ArtistId = ps.Song.ArtistId,
                        ArtistName = ps.Song.Artist.Name ?? ps.Song.Artist.UserName,
                        IsActive = ps.Song.IsActive,
                        UploadedAt = ps.Song.UploadedAt,
                        PlayCount = ps.Song.PlaybackStatistics.Sum(stat => stat.PlayCount)
                    }).ToList()
                })
                .ToListAsync();

            return Ok(playlists);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PlaylistDto>> GetPlaylist(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var playlist = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
                return NotFound();

            if (playlist.UserId != userId && !isAdmin)
                return Forbid();

            var playlistDto = new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                UserId = playlist.UserId,
                UserName = playlist.User.Name ?? playlist.User.UserName,
                Songs = playlist.PlaylistSongs.Select(ps => new SongDto
                {
                    Id = ps.Song.Id,
                    Title = ps.Song.Title,
                    Duration = ps.Song.Duration,
                    Genre = ps.Song.Genre,
                    StreamingUrl = ps.Song.StreamingUrl,
                    ArtistId = ps.Song.ArtistId,
                    ArtistName = ps.Song.Artist.Name ?? ps.Song.Artist.UserName,
                    IsActive = ps.Song.IsActive,
                    UploadedAt = ps.Song.UploadedAt,
                    PlayCount = ps.Song.PlaybackStatistics.Sum(stat => stat.PlayCount)
                }).ToList()
            };

            return Ok(playlistDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PlaylistDto>> CreatePlaylist(CreatePlaylistDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var playlist = new Playlist
            {
                Name = dto.Name,
                UserId = userId
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            var playlistDto = new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                UserId = playlist.UserId,
                UserName = user?.Name ?? user?.UserName,
                Songs = new List<SongDto>()
            };

            return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlistDto);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlaylist(int id, UpdatePlaylistDto dto)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (playlist.UserId != userId && !isAdmin)
                return Forbid();

            if (dto.Name != null) playlist.Name = dto.Name;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Playlist updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (playlist.UserId != userId && !isAdmin)
                return Forbid();

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Playlist deleted successfully" });
        }

        [HttpPost("{id}/songs")]
        [Authorize]
        public async Task<IActionResult> AddSongToPlaylist(int id, AddSongToPlaylistDto dto)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (playlist.UserId != userId)
                return Forbid();

            var song = await _context.Songs.FindAsync(dto.SongId);
            if (song == null || !song.IsActive)
                return BadRequest(new { message = "Song not found or inactive" });

            var existingPlaylistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == dto.SongId);

            if (existingPlaylistSong != null)
                return BadRequest(new { message = "Song already in playlist" });

            var playlistSong = new PlaylistSong
            {
                PlaylistId = id,
                SongId = dto.SongId
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Song added to playlist successfully" });
        }

        [HttpDelete("{id}/songs/{songId}")]
        [Authorize]
        public async Task<IActionResult> RemoveSongFromPlaylist(int id, int songId)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (playlist.UserId != userId)
                return Forbid();

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == songId);

            if (playlistSong == null)
                return NotFound();

            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Song removed from playlist successfully" });
        }
    }
}
