using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebDoDienTu.Models;
using WebDoDienTu.Models.Repository;

namespace WebDoDienTu.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy dữ liệu từ database và gán cho MyModel
            MyModel model = new MyModel();
            // Gán dữ liệu vào MyModel
            model.Products = _context.Products.ToList();
            model.Categories = _context.Categories.ToList();

            return View(model);
        }
    }
}
