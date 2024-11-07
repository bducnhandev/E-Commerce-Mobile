using System.Text.Json.Serialization;

namespace WebDoDienTu.Models
{
    public class ProductAttribute
    {
        public int ProductAttributeId { get; set; }
        public int ProductId { get; set; }
        public string AttributeName { get; set; } = String.Empty;
        public string AttributeValue { get; set; } = String.Empty;

        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
