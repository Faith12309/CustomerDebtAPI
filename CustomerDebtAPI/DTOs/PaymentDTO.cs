using System.ComponentModel.DataAnnotations;

namespace CustomerDebtAPI.DTOs
{
    public class PaymentDTO
    {
        [Required]
        public decimal PaymentAmount { get; set; }
    }
}