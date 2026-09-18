namespace CustomerDebtAPI.DTOs
{
    public class SaleResponseDTO
    {
        public int SaleId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public bool IsCredit { get; set; }

        public DateTime DateSold { get; set; }
    }
}