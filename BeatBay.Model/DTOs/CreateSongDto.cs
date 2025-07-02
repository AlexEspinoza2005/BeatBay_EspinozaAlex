using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class CreateSongDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [MaxLength(100)]
        public string? Genre { get; set; }

        [Required]
        public string StreamingUrl { get; set; }
    }
}
