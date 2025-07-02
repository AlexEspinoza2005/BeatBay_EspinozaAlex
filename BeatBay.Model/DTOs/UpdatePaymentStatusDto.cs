using BeatBay.Model;
using System.ComponentModel.DataAnnotations;

namespace BeatBay.DTOs
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        public PaymentStatus Status { get; set; }
    }
}
