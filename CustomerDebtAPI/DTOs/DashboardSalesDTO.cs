namespace CustomerDebtAPI.DTOs
{
    public class DashboardSalesDTO
    {
        public decimal TodaySales { get; set; }
        public decimal CashSales { get; set; }
        public decimal CreditSales { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
    }
}