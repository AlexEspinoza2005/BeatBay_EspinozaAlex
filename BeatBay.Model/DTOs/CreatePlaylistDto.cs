using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class CreatePlaylistDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; }
    }
}
