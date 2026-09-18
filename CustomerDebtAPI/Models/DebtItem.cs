using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomerDebtAPI.Models
{
    public class DebtItem
    {
        [Key]
        public int Id { get; set; }

        public int DebtId { get; set; }

        [ForeignKey("DebtId")]
        public Debt? Debt { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }
    }
}