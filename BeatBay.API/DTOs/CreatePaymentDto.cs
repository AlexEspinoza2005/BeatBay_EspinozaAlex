using System.ComponentModel.DataAnnotations;
using BeatBay.Model;

namespace BeatBay.API.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
