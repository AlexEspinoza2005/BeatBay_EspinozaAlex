using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class UpdatePlanDto
    {
        public string? Name { get; set; }
        public decimal? PriceUSD { get; set; }
        public int? MaxConnections { get; set; }
    }
}
