using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;
using WebDoDienTu.Models.Repository;
using X.PagedList;

namespace WebDoDienTu.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? category, string? keywords, string? priceRange, int? page)
        {
            int pageSize = 9;
            int pageNumber = (page ?? 1);

            IPagedList<Product> products;

            if (!string.IsNullOrEmpty(category))
            {
                products = _context.Products
                    .Include(p => p.Category)
                    .AsEnumerable()
                    .Where(x => x.Category.CategoryName
                    .Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToPagedList(pageNumber, pageSize);

                if (!products.Any())
                {
                    TempData["NoProductsMessage"] = "No suitable products found.";
                }
            }
            else if (!string.IsNullOrEmpty(keywords))
            {
                products = _context.Products.Where(x => x.ProductName.Contains(keywords)).ToPagedList(pageNumber, pageSize);

                if (!products.Any())
                {
                    TempData["NoProductsMessage"] = "No suitable products found.";
                }
            }
            else if (!string.IsNullOrEmpty(priceRange))
            {
                var priceLimits = priceRange.Split('-').Select(int.Parse).ToList();
                var minPrice = priceLimits[0];
                var maxPrice = priceLimits[1];

                products = _context.Products.Where(x => x.Price >= minPrice && x.Price < maxPrice).ToPagedList(pageNumber, pageSize);

                if (!products.Any())
                {
                    TempData["NoProductsMessage"] = "No suitable products found.";
                }
            }
            else
            {
                products = _context.Products.ToPagedList(pageNumber, pageSize);

                if (!products.Any())
                {
                    TempData["NoProductsMessage"] = "No suitable products found.";
                }
            }

            ViewBag.Category = category;
            ViewBag.Keywords = keywords;
            ViewBag.PriceRange = priceRange;

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(x => x.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
