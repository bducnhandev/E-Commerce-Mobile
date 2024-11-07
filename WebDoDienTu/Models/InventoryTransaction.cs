using System.ComponentModel.DataAnnotations;

namespace WebDoDienTu.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Quantity { get; set; }
        public TransactionType TransactionType { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
