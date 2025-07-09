using BeatBay.Data;
using BeatBay.DTOs;
using BeatBay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BeatBay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistsController : ControllerBase
    {
        private readonly BeatBayDbContext _context;
        private readonly UserManager<User> _userManager;

        public PlaylistsController(BeatBayDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/playlists (Playlists públicas)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlaylistDto>>> GetPlaylists()
        {
            var playlists = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
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

        // GET: api/playlists/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlaylistDto>> GetPlaylist(int id)
        {
            var playlist = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (playlist == null)
                return NotFound();

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

        // GET: api/playlists/my-playlists - SIN AUTHORIZE
        [HttpGet("my-playlists")]
        public async Task<ActionResult<IEnumerable<PlaylistDto>>> GetMyPlaylists()
        {
            // Usar userId 1 por defecto si no hay usuario autenticado
            int userId = 1;

            var playlists = await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs)
                    .ThenInclude(ps => ps.Song)
                        .ThenInclude(s => s.Artist)
                .Where(p => p.UserId == userId)
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

        // POST: api/playlists - SIN AUTHORIZE
        [HttpPost]
        public async Task<ActionResult<PlaylistDto>> CreatePlaylist(CreatePlaylistDto dto)
        {
            // Usar userId 1 por defecto
            int userId = 1;

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
                UserName = user?.Name ?? user?.UserName ?? "Usuario",
                Songs = new List<SongDto>()
            };

            return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlistDto);
        }

        // PUT: api/playlists/5 - SIN AUTHORIZE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlaylist(int id, CreatePlaylistDto dto)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound();

            // Quitar validaciones de usuario - permitir editar cualquier playlist
            playlist.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Playlist actualizada correctamente" });
        }

        // DELETE: api/playlists/5 - SIN AUTHORIZE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound();

            // Quitar validaciones de usuario - permitir eliminar cualquier playlist
            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Playlist eliminada correctamente" });
        }

        // POST: api/playlists/5/songs - SIN AUTHORIZE
        [HttpPost("{id}/songs")]
        public async Task<IActionResult> AddSongToPlaylist(int id, AddSongToPlaylistDto dto)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound("Playlist no encontrada.");

            var song = await _context.Songs.FindAsync(dto.SongId);
            if (song == null)
                return NotFound("Canción no encontrada.");

            // Quitar validación de IsActive
            // if (!song.IsActive)
            //     return BadRequest("No se puede agregar una canción inactiva.");

            // Verificar si la canción ya está en la playlist
            var existingPlaylistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == dto.SongId);

            if (existingPlaylistSong != null)
                return BadRequest("La canción ya está en la playlist.");

            var playlistSong = new PlaylistSong
            {
                PlaylistId = id,
                SongId = dto.SongId
            };

            _context.PlaylistSongs.Add(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Canción agregada a la playlist correctamente" });
        }

        // DELETE: api/playlists/5/songs/3 - SIN AUTHORIZE
        [HttpDelete("{id}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(int id, int songId)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist == null)
                return NotFound("Playlist no encontrada.");

            var playlistSong = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == id && ps.SongId == songId);

            if (playlistSong == null)
                return NotFound("La canción no está en la playlist.");

            // Quitar validaciones de usuario - permitir remover de cualquier playlist
            _context.PlaylistSongs.Remove(playlistSong);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Canción removida de la playlist correctamente" });
        }
    }
}