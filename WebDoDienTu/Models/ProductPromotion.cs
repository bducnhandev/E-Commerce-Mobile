namespace WebDoDienTu.Models
{
    public class ProductPromotion
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; }
    }
}