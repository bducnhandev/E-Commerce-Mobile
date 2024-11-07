using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;
using WebDoDienTu.Models.Repository;
using WebDoDienTu.Data;

namespace WebDoDienTu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly Cloudinary _cloudinary;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment environment, ApplicationDbContext context, Cloudinary cloudinary)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _environment = environment;
            _context = context;
            _cloudinary = cloudinary;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // Hiển thị form thêm sản phẩm mới
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // Xử lý thêm sản phẩm mới
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl, IFormFile videoUrl, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                if (videoUrl != null)
                {
                    product.VideoUrl = await SaveVideo(videoUrl);
                }

                if (images != null)
                {
                    product.Images = new List<ProductImage>();
                    foreach (var img in images)
                    {
                        var url = await SaveImage(img);
                        product.Images.Add(new ProductImage { Url = url });
                    }

                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        private async Task<string?> SaveVideo(IFormFile video)
        {
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(video.FileName, video.OpenReadStream()),
                PublicId = $"products/videos/{Guid.NewGuid()}"
            };

            var uploadResult = await _cloudinary.UploadLargeAsync<VideoUploadResult>(uploadParams, 5000000, 5);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString(); 
            }

            return null;
        }

        private async Task<string?> SaveImage(IFormFile image)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName, image.OpenReadStream()),
                PublicId = $"products/{Guid.NewGuid()}"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            return null;
        }

        // Hiển thị thông tin chi tiết sản phẩm
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            ViewBag.listImages = product.Images;
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Hiển thị form cập nhật sản phẩm
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile imageUrl, IFormFile videoUrl, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Cập nhật hình ảnh đại diện nếu có ảnh mới
                if (imageUrl != null)
                {
                    existingProduct.ImageUrl = await SaveImage(imageUrl);
                }

                // Cập nhật các thuộc tính khác của sản phẩm
                existingProduct.ProductName = product.ProductName;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.IsHoted = product.IsHoted;

                // Xóa các ảnh cũ nếu có ảnh mới được cung cấp
                if (images != null && images.Count > 0)
                {
                    // Xóa các ảnh cũ khỏi cơ sở dữ liệu
                    _context.ProductImages.RemoveRange(existingProduct.Images);

                    // Lưu các ảnh mới
                    existingProduct.Images = new List<ProductImage>();
                    foreach (var img in images)
                    {
                        var url = await SaveImage(img);
                        existingProduct.Images.Add(new ProductImage { Url = url });
                    }
                }

                // Cập nhật video nếu có video mới
                if (videoUrl != null)
                {
                    existingProduct.VideoUrl = await SaveVideo(videoUrl);
                }

                _context.Update(existingProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
