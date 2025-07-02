using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class PlaybackStatisticDto
    {
        public int Id { get; set; }
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
        public int? SongId { get; set; }
        public string? SongTitle { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime PlaybackDate { get; set; }
        public int DurationPlayedSeconds { get; set; }
        public int PlayCount { get; set; }
    }
}
