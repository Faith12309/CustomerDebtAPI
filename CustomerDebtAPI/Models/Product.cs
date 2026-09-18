using CustomerDebtAPI.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Category { get; set; } = "";

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Unit { get; set; } = "";
    public bool IsActive { get; set; } = true;

    public ICollection<DebtItem> DebtItems { get; set; } = new List<DebtItem>();
}