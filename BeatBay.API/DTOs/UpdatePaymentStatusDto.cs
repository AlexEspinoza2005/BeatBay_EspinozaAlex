using BeatBay.Model;
using System.ComponentModel.DataAnnotations;

namespace BeatBay.API.DTOs
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        public PaymentStatus Status { get; set; }
    }
}
