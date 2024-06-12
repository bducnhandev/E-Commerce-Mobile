using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;

namespace WebDoDienTu.Controllers
{
    public class BlogController : Controller
    {
        private ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            MyModel model = new MyModel();
            // Gán dữ liệu vào MyModel
            model.Products = _context.Products.ToList();
            model.Categories = _context.Categories.ToList();

            return View();
        }
    }
}
