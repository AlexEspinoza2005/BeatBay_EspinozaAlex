using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeatBay.Model
{
    public class Song
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [MaxLength(100)]
        public string? Genre { get; set; }

        [Required]
        public string StreamingUrl { get; set; }  // URL para streaming externo

        [ForeignKey("Artist")]
        public int ArtistId { get; set; }

        public virtual User Artist { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PlaybackStatistic> PlaybackStatistics { get; set; } = new HashSet<PlaybackStatistic>();
    }
}
