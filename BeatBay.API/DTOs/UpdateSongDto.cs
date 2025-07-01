using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class UpdateSongDto
    {
        public string? Title { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Genre { get; set; }
        public string? StreamingUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
