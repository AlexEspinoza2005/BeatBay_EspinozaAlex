using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class TopSongsDto
    {
        public List<SongStatsDto> Songs { get; set; } = new List<SongStatsDto>();
    }
}
