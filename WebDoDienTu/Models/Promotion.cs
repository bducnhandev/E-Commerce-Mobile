namespace WebDoDienTu.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; }  // Mã khuyến mãi
        public decimal DiscountAmount { get; set; }  // Giảm giá cố định
        public decimal DiscountPercentage { get; set; }  // Giảm giá theo phần trăm
        public decimal MinimumOrderAmount { get; set; }  // Giá trị tối thiểu của đơn hàng
        public bool IsPercentage { get; set; }  // Xác định nếu giảm giá theo phần trăm
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public PromotionType Type { get; set; }  // Sản phẩm hoặc đơn hàng
    }
}
