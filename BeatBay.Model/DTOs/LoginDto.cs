using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class LoginDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
