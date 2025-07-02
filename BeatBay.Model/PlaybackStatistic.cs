using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeatBay.Model
{
    public enum EntityType
    {
        Global = 0,
        Plan = 1,
        Song = 2,
        User = 3
    }

    public class PlaybackStatistic
    {
        [Key]
        public int Id { get; set; }

        // Tipo de entidad a la que se refiere esta estadística
        [Required]
        public EntityType EntityType { get; set; }

        // ID de la entidad (por ejemplo, Id de canción, usuario o plan)
        [Required]
        public int EntityId { get; set; }

        // Enlace opcional a la canción (solo si EntityType == Song)
        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }
        public int? SongId { get; set; }

        // Enlace opcional al usuario (solo si EntityType == User)
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public int? UserId { get; set; }

        // Fecha y hora de la reproducción (puede ser para análisis de tiempos)
        public DateTime PlaybackDate { get; set; }

        // Duración reproducida en segundos
        public int DurationPlayedSeconds { get; set; }

        // Métrica general: número total de reproducciones (puede usarse para estadísticas agregadas)
        public int PlayCount { get; set; }

        // Constructor vacío requerido por EF Core
        public PlaybackStatistic()
        {
        }

        // Constructor de conveniencia
        public PlaybackStatistic(EntityType entityType, int entityId, DateTime playbackDate, int durationPlayedSeconds, int playCount = 1)
        {
            EntityType = entityType;
            EntityId = entityId;
            PlaybackDate = playbackDate;
            DurationPlayedSeconds = durationPlayedSeconds;
            PlayCount = playCount;
        }
    }
}
