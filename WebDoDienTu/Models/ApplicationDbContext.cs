using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Export.HtmlExport.StyleCollectors.StyleContracts;

namespace WebDoDienTu.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<WishListItem> WishListItem { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ProductPromotion> ProductPromotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductPromotion>()
                .HasKey(pp => new { pp.ProductId, pp.PromotionId });

            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.ProductId);

            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Promotion)
                .WithMany()
                .HasForeignKey(pp => pp.PromotionId);

            // Một sản phẩm có nhiều đánh giá
            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId);

            // Một đơn hàng có thể có nhiều mục sản phẩm
            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(oi => oi.OrderId);

            // Khuyến mãi có thể áp dụng cho đơn hàng
            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderPromotion)
                .WithMany()
                .HasForeignKey(o => o.OrderPromotionId);

            modelBuilder.Entity<WishListItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.ProductId);

            modelBuilder.Entity<WishListItem>()
                .HasOne(wi => wi.WishList)
                .WithMany(w => w.WishListItems)
                .HasForeignKey(wi => wi.WishListId);

            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId);

            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.User)
                .WithMany()
                .HasForeignKey(pr => pr.UserId);

            // Thêm dữ liệu cho Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Điện thoại" },
                new Category { CategoryId = 2, CategoryName = "Tai nghe" }
            );

            // Thêm dữ liệu cho Products
            modelBuilder.Entity<Product>().HasData(
                new Product { 
                    ProductId = 1, 
                    ProductName = "IPhone 13 Pro Max", 
                    Price = 13999999, 
                    Description = "IPhone 13 Pro Max, 128GB, Màu Xám", 
                    ImageUrl = "/image/products/phone/iPhone 13 Pro Max.png",
                    IsHoted = false, 
                    CategoryId = 1
                },
                new Product { 
                    ProductId = 2, 
                    ProductName = "Apple AirPods Pro", 
                    Price = 6990000,
                    Description = "Tai nghe Apple AirPods Pro, Chống ồn, Bluetooth",
                    ImageUrl = "/image/products/sound/Apple AirPods Pro.jpg",
                    IsHoted = false, 
                    CategoryId = 2},
                new Product {
                    ProductId = 3, 
                    ProductName = "Samsung Galaxy S21 Ultra",
                    Price = 28990000,
                    Description = "Samsung Galaxy S21 Ultra, 256GB, Màu Đen",
                    ImageUrl = "/image/products/phone/Samsung Galaxy S21 Ultra.jpg",
                    IsHoted = false,
                    CategoryId = 1}
            );

            // Thêm dữ liệu cho Vouchers
            //modelBuilder.Entity<Voucher>().HasData(
            //    new Voucher { Id = 1, Name = "Summer Sale", Description = "Giảm giá mùa hè", Code = "SUMMER2024", ExpiryDate = DateTime.Parse("8/31/2024"), Value = 20, SoLuong = 100 },
            //    new Voucher { Id = 2, Name = "New Year Discount", Description = "Giảm giá đầu năm", Code = "NEWYEAR2025", ExpiryDate = DateTime.Parse("1/31/2025"), Value = 15, SoLuong = 150 },
            //    new Voucher { Id = 3, Name = "Black Friday", Description = "Giảm giá Black Friday", Code = "BLACKFRIDAY2024", ExpiryDate = DateTime.Parse("11/29/2024"), Value = 30, SoLuong = 99 },
            //    new Voucher { Id = 4, Name = "Christmas Offer", Description = "Ưu đãi Giáng sinh", Code = "CHRISTMAS2024", ExpiryDate = DateTime.Parse("12/25/2024"), Value = 25, SoLuong = 99 },
            //    new Voucher { Id = 5, Name = "Mid-Autumn Festival", Description = "Giảm giá Tết Trung Thu", Code = "MIDAUTUMN2024", ExpiryDate = DateTime.Parse("9/21/2024"), Value = 10, SoLuong = 80 }
            //);

            // Dữ liệu mẫu cho bảng Promotion
            modelBuilder.Entity<Promotion>().HasData(
                new Promotion
                {
                    Id = 1,
                    Code = "PROD10",
                    DiscountAmount = 10,
                    DiscountPercentage = 0,
                    MinimumOrderAmount = 0,
                    IsPercentage = false,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(10),
                    IsActive = true,
                    Type = PromotionType.Product
                },
                new Promotion
                {
                    Id = 2,
                    Code = "ORDER15",
                    DiscountAmount = 0,
                    DiscountPercentage = 15,
                    MinimumOrderAmount = 100,
                    IsPercentage = true,
                    StartDate = DateTime.Now.AddDays(-5),
                    EndDate = DateTime.Now.AddDays(15),
                    IsActive = true,
                    Type = PromotionType.Order
                },
                new Promotion
                {
                    Id = 3,
                    Code = "ORDER50",
                    DiscountAmount = 50,
                    DiscountPercentage = 0,
                    MinimumOrderAmount = 200,
                    IsPercentage = false,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(20),
                    IsActive = true,
                    Type = PromotionType.Order
                },
                new Promotion
                {
                    Id = 4,
                    Code = "PROD5",
                    DiscountAmount = 0,
                    DiscountPercentage = 5,
                    MinimumOrderAmount = 0,
                    IsPercentage = true,
                    StartDate = DateTime.Now.AddDays(-30),
                    EndDate = DateTime.Now.AddDays(30),
                    IsActive = true,
                    Type = PromotionType.Product
                }
            );
        }
    }
}
