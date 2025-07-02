using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeatBay.Model
{
    public class PlaylistSong
    {
        public int PlaylistId { get; set; }
        public virtual Playlist Playlist { get; set; }

        public int SongId { get; set; }
        public virtual Song Song { get; set; }
    }
}
