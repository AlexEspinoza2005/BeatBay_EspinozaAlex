using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeatBay.Model
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        [ForeignKey("Plan")]
        public int PlanId { get; set; }

        public virtual Plan Plan { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
    }
}
