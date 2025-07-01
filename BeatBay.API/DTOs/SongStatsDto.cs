using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class SongStatsDto
    {
        public int SongId { get; set; }
        public string Title { get; set; }
        public string ArtistName { get; set; }
        public int PlayCount { get; set; }
        public int TotalDurationPlayed { get; set; }
    }
}
