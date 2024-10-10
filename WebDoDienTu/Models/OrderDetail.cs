namespace WebDoDienTu.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }

        public decimal GetDiscountedPrice()
        {
            var promotion = Product.ProductPromotions.FirstOrDefault(pp => pp.Promotion.IsActive
                                                                        && pp.Promotion.StartDate <= DateTime.Now
                                                                        && pp.Promotion.EndDate >= DateTime.Now)?.Promotion;

            if (promotion != null && promotion.Type == PromotionType.Product)
            {
                return promotion.IsPercentage ?
                       Price * (1 - promotion.DiscountPercentage / 100) :
                       Price - promotion.DiscountAmount;
            }
            return Price;
        }

        public void ApplyProductDiscount(Promotion promotion)
        {
            if (promotion.IsPercentage)
            {
                this.DiscountAmount = this.Price * promotion.DiscountPercentage / 100;
            }
            else
            {
                this.DiscountAmount = promotion.DiscountAmount;
            }

            this.DiscountedPrice = this.Price - this.DiscountAmount;
        }
    }
}
