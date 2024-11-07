using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;

namespace WebDoDienTu.Data
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
        public DbSet<WishListItem> WishListItems { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<OrderComplaint> OrderComplaints { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductView> ProductViews { get; set; }
        public DbSet<ProductRecommendation> ProductRecommendations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(oi => oi.OrderId);

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

            modelBuilder.Entity<OrderComplaint>()
                .HasOne(oc => oc.Order)
                .WithMany(o => o.Complaints)
                .HasForeignKey(oc => oc.OrderId);

            modelBuilder.Entity<OrderComplaint>()
                .HasOne(oc => oc.User)
                .WithMany()
                .HasForeignKey(oc => oc.UserId);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<PostCategory>()
                .HasMany(c => c.Posts)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Attributes)
                .WithOne(a => a.Product)
                .HasForeignKey(a => a.ProductId);

            // Thêm dữ liệu cho Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Điện thoại" },
                new Category { CategoryId = 2, CategoryName = "Tai nghe" }
            );

            // Thêm dữ liệu cho Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    ProductName = "IPhone 13 Pro Max",
                    Price = 13999999,
                    Description = "IPhone 13 Pro Max, 128GB, Màu Xám",
                    ImageUrl = "/image/products/phone/iPhone 13 Pro Max.png",
                    IsHoted = true,
                    CategoryId = 1
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Apple AirPods Pro",
                    Price = 6990000,
                    Description = "Tai nghe Apple AirPods Pro, Chống ồn, Bluetooth",
                    ImageUrl = "/image/products/sound/Apple AirPods Pro.jpg",
                    IsHoted = true,
                    CategoryId = 2
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Samsung Galaxy S21 Ultra",
                    Price = 28990000,
                    Description = "Samsung Galaxy S21 Ultra, 256GB, Màu Đen",
                    ImageUrl = "/image/products/phone/Samsung Galaxy S21 Ultra.jpg",
                    IsHoted = true,
                    CategoryId = 1
                }
            );

            // Dữ liệu mẫu cho bảng ProductAttributes
            modelBuilder.Entity<ProductAttribute>().HasData(
                // IPhone 13 Pro Max
                new ProductAttribute { ProductAttributeId = 1, ProductId = 1, AttributeName = "Màu sắc", AttributeValue = "Xám" },
                new ProductAttribute { ProductAttributeId = 2, ProductId = 1, AttributeName = "Dung lượng", AttributeValue = "128GB" },
                new ProductAttribute { ProductAttributeId = 3, ProductId = 1, AttributeName = "Kích thước màn hình", AttributeValue = "6.7 inch" },
                new ProductAttribute { ProductAttributeId = 4, ProductId = 1, AttributeName = "Thời gian sử dụng pin", AttributeValue = "20 giờ" },

                // Apple AirPods Pro
                new ProductAttribute { ProductAttributeId = 5, ProductId = 2, AttributeName = "Màu sắc", AttributeValue = "Trắng" },
                new ProductAttribute { ProductAttributeId = 6, ProductId = 2, AttributeName = "Thời gian sử dụng pin", AttributeValue = "4.5 giờ" },
                new ProductAttribute { ProductAttributeId = 7, ProductId = 2, AttributeName = "Công nghệ chống ồn", AttributeValue = "Có" },
                new ProductAttribute { ProductAttributeId = 8, ProductId = 2, AttributeName = "Trọng lượng", AttributeValue = "5.4 gram" },

                // Samsung Galaxy S21 Ultra
                new ProductAttribute { ProductAttributeId = 9, ProductId = 3, AttributeName = "Màu sắc", AttributeValue = "Đen" },
                new ProductAttribute { ProductAttributeId = 10, ProductId = 3, AttributeName = "Dung lượng", AttributeValue = "256GB" },
                new ProductAttribute { ProductAttributeId = 11, ProductId = 3, AttributeName = "Kích thước màn hình", AttributeValue = "6.8 inch" },
                new ProductAttribute { ProductAttributeId = 12, ProductId = 3, AttributeName = "Thời gian sử dụng pin", AttributeValue = "22 giờ" }
            );


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
                }
            );
        }
    }
}
