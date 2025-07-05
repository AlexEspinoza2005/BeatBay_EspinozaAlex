using System;
using System.ComponentModel.DataAnnotations;

namespace BeatBay.DTOs
{
    public class SongDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Genre { get; set; }
        public string StreamingUrl { get; set; } 
        public int ArtistId { get; set; }
        public string ArtistName { get; set; }
        public bool IsActive { get; set; }
        public DateTime UploadedAt { get; set; }
        public int PlayCount { get; set; }
    }
}
