using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebDoDienTu.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(50)]
        [DisplayName("Tên danh mục")]
        public string CategoryName { get; set; }
        public List<Product> Products { get; set; }
    }
}
