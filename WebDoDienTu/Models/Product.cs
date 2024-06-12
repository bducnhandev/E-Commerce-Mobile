using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebDoDienTu.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required, StringLength(100)]
        [DisplayName("Tên sản phẩm")]
        public string ProductName { get; set; }

        [DisplayName("Giá")]
        public int Price { get; set; }

        [DisplayName("Mô tả")]
        public string Description { get; set; }

        [DisplayName("Hình ảnh")]
        public string? ImageUrl { get; set; }

        [DisplayName("Ảnh khác")]
        public List<ProductImage>? Images { get; set; }

        [DisplayName("Sản phẩm hot")]
        public bool? IsHoted { get; set; }

        [ForeignKey("Category")]
        [DisplayName("Loại sản phẩm")]
        public int CategoryId { get; set; }

        [DisplayName("Loại sản phẩm")]
        public Category? Category { get; set; }
    }
}
