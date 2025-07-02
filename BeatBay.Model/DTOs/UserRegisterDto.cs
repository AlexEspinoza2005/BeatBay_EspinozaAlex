using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class UserRegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
    }
}
