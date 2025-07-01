using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using Microsoft.AspNetCore.Identity;

namespace BeatBay.Model
{
    public class User : IdentityUser<int>
    {
        // Nombre completo
        [MaxLength(100)]
        public string? Name { get; set; }

        // Biografía o descripción corta
        public string? Bio { get; set; }

        // Relación con Plan (nullable para Free users)
        public int? PlanId { get; set; }
        public virtual Plan? Plan { get; set; }

        // Control de estado (activo/bloqueado)
        public bool IsActive { get; set; } = true;

        // Fecha de creación (UTC)
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Dominio de playlists, canciones subidas y pagos
        public virtual ICollection<Playlist> Playlists { get; set; } = new HashSet<Playlist>();
        public virtual ICollection<Song> UploadedSongs { get; set; } = new HashSet<Song>();
        public virtual ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
    }
}
