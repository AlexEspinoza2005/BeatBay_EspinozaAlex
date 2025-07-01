using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class PlanStatsDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public int UserCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
