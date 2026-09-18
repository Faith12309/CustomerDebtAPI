public class DashboardDTO
{
    public int TotalCustomers { get; set; }
    public int TotalDebts { get; set; }
    public int PaidDebts { get; set; }
    public int UnpaidDebts { get; set; }
    public int PartialPaidDebts { get; set; }
    public int OverdueDebts { get; set; }

    public int TotalProducts { get; set; }

    public decimal OutstandingDebt { get; set; }
}