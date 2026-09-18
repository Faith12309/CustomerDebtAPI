using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerDebtAPI.Models
{
    public class Debt
    {
        [Key]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public decimal RemainingBalance { get; set; }

        public string Status { get; set; } = "Unpaid";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<DebtItem> DebtItems { get; set; } = new List<DebtItem>();
    }
}