using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebDoDienTu.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, StringLength(100)]
        [DisplayName("Tên sản phẩm")]
        public string ProductName { get; set; } = String.Empty;

        [DisplayName("Giá")]
        public int Price { get; set; }

        [DisplayName("Mô tả")]
        public string Description { get; set; } = String.Empty;

        [DisplayName("Hình ảnh")]
        public string? ImageUrl { get; set; }

        [DisplayName("Video")]
        public string? VideoUrl { get; set; }
        
        [DisplayName("Ảnh khác")]
        public List<ProductImage>? Images { get; set; }

        [DisplayName("Sản phẩm hot")]
        public bool? IsHoted { get; set; }
        public int StockQuantity { get; set; }
        public bool IsPreOrder { get; set; }
        public DateTime? ReleaseDate { get; set; }

        [ForeignKey("Category")]
        [DisplayName("Loại sản phẩm")]
        public int CategoryId { get; set; }

        [DisplayName("Loại sản phẩm")]
        public Category? Category { get; set; }

        public ICollection<ProductReview>? Reviews { get; set; }
        public ICollection<InventoryTransaction>? InventoryTransactions { get; set; }
        [JsonIgnore]
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    }
}
