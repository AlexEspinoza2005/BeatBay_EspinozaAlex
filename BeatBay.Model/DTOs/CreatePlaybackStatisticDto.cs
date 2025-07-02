using BeatBay.Model;
using System.ComponentModel.DataAnnotations;

namespace BeatBay.DTOs
{
    public class CreatePlaybackStatisticDto
    {
        [Required]
        public EntityType EntityType { get; set; }

        [Required]
        public int EntityId { get; set; }

        public int? SongId { get; set; }
        public int? UserId { get; set; }

        [Required]
        public int DurationPlayedSeconds { get; set; }

        public int PlayCount { get; set; } = 1;
    }
}
