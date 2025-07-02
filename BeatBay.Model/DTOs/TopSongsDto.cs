using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.DTOs
{
    public class TopSongsDto
    {
        public List<SongStatsDto> Songs { get; set; } = new List<SongStatsDto>();
    }
}
