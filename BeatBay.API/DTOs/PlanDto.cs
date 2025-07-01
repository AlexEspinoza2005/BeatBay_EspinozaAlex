using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class PlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal PriceUSD { get; set; }
        public int MaxConnections { get; set; }
        public int UserCount { get; set; }
    }
}
