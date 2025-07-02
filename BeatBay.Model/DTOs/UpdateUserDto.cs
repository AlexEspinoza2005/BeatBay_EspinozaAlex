using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public int? PlanId { get; set; }
    }
}
