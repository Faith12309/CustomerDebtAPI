namespace CustomerDebtAPI.DTOs
{
    public class DebtResponseDTO
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string DueDate { get; set; } = string.Empty;

        public decimal RemainingBalance { get; set; }

        public string Status { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;
    }
}