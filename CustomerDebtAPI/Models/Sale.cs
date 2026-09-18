namespace CustomerDebtAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsCredit { get; set; }

        public DateTime DateSold { get; set; } = DateTime.Now;

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}