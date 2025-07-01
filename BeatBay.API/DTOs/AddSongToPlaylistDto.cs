using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class AddSongToPlaylistDto
    {
        [Required]
        public int SongId { get; set; }
    }
}
