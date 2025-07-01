using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class StatisticsSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalSongs { get; set; }
        public int TotalPlaylists { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<PlanStatsDto> PlanStats { get; set; } = new List<PlanStatsDto>();
    }
}
