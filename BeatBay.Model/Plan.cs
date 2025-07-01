using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeatBay.Model
{
    public class Plan
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }  // Ej: Personal, Familiar, Empresarial

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceUSD { get; set; }

        [Required]
        public int MaxConnections { get; set; }

        public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
        public virtual ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
    }
}
