namespace CustomerDebtAPI.DTOs
{
    public class SaleDTO
    {
        public int? CustomerId { get; set; }

        public bool IsCredit { get; set; }

        public List<SaleItemDTO> Items { get; set; } = new();

        public DateTime? DueDate { get; set; }
    }
}