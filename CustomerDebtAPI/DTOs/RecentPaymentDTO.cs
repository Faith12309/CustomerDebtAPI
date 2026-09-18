namespace CustomerDebtAPI.DTOs
{
    public class RecentPaymentDTO
    {
        public string CustomerName { get; set; } = string.Empty;

        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; }
    }
}