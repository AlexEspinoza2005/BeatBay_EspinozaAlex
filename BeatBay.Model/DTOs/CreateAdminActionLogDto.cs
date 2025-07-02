using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class CreateAdminActionLogDto
    {
        [Required]
        public string ActionType { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
