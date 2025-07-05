using Azure.Storage.Blobs;
using BeatBay.DTOs;
using BeatBay.Data;
using BeatBay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly BlobContainerClient _blobContainerClient;

        public SongsController(BeatBayDbContext context, IOptions<AzureBlobStorageSettings> blobSettings)
        {
            _context = context;

            // Crear BlobContainerClient con la URL SAS de tu contenedor, por url sin json
            _blobContainerClient = new BlobContainerClient(new Uri("https://musicauploadalex.blob.core.windows.net/audios?sp=racwdli&st=2025-07-04T01:33:25Z&se=2025-07-04T09:33:25Z&sv=2024-11-04&sr=c&sig=DwX%2BQqu4nfqmW1hMi7pZ9TS0SuvZX4l55Wgr9UWsK6Q%3D"));

        }

        // GET: api/songs?isActive=true
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

        // GET: api/songs/5
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

        // POST: api/songs
        [HttpPost]
       // [Authorize(Roles = "Artist,Admin")]
        public async Task<ActionResult<SongDto>> CreateSong([FromForm] CreateSongDto dto, IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest("No se ha seleccionado un archivo de audio.");

            var allowedExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a" };
            var fileExtension = Path.GetExtension(audioFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Formato de archivo no soportado. Solo se permiten: mp3, wav, flac, m4a");

            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            try
            {
                // Subir archivo a Azure Blob Storage
                var blobClient = _blobContainerClient.GetBlobClient(uniqueFileName);
                using var stream = audioFile.OpenReadStream();
                await blobClient.UploadAsync(stream);

                var streamingUrl = blobClient.Uri.ToString();

                // Obtener el ArtistId desde el token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int artistId))
                    return Unauthorized("No se pudo obtener el ID del artista.");

                var song = new Song
                {
                    Title = dto.Title,
                    Duration = dto.Duration,
                    Genre = dto.Genre,
                    StreamingUrl = streamingUrl,
                    ArtistId = artistId,
                    IsActive = true,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Songs.Add(song);
                await _context.SaveChangesAsync();

                var songDto = new SongDto
                {
                    Id = song.Id,
                    Title = song.Title,
                    Duration = song.Duration,
                    Genre = song.Genre,
                    StreamingUrl = song.StreamingUrl,
                    ArtistId = song.ArtistId,
                    ArtistName = null, // Opcional: puedes cargar nombre si quieres
                    IsActive = song.IsActive,
                    UploadedAt = song.UploadedAt,
                    PlayCount = 0
                };

                return CreatedAtAction(nameof(GetSong), new { id = song.Id }, songDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al subir el archivo: {ex.Message}");
            }
        }

        // PUT: api/songs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> UpdateSong(int id, UpdateSongDto dto)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            bool isAdmin = User.IsInRole("Admin");

            if (song.ArtistId != userId && !isAdmin)
                return Forbid();

            if (!string.IsNullOrWhiteSpace(dto.Title))
                song.Title = dto.Title;

            if (dto.Duration.HasValue)
                song.Duration = dto.Duration.Value;

            if (!string.IsNullOrWhiteSpace(dto.Genre))
                song.Genre = dto.Genre;

            if (!string.IsNullOrWhiteSpace(dto.StreamingUrl))
                song.StreamingUrl = dto.StreamingUrl;

            if (dto.IsActive.HasValue)
                song.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Canción actualizada correctamente" });
        }

        // DELETE: api/songs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Artist,Admin")]
        public async Task<IActionResult> DeleteSong(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song == null)
                return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            bool isAdmin = User.IsInRole("Admin");

            if (song.ArtistId != userId && !isAdmin)
                return Forbid();

            song.IsActive = false;
            await _context.SaveChangesAsync();

            // Opcional: registrar log si lo borró un admin distinto al artista
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

            return Ok(new { message = "Canción desactivada correctamente" });
        }
    }

    // Clase para inyectar configuración de Azure Blob
    public class AzureBlobStorageSettings
    {
        public string ContainerUrl { get; set; }
    }
}
