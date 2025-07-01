using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public int? PlanId { get; set; }
        public string? PlanName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
