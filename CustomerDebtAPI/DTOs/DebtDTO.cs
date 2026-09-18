public class DebtDTO
{
    public int CustomerId { get; set; }

    public decimal Amount { get; set; }

    public string DueDate { get; set; } = string.Empty;
}