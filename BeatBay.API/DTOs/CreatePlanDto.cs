using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class CreatePlanDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required]
        public decimal PriceUSD { get; set; }

        [Required]
        public int MaxConnections { get; set; }
    }
}
