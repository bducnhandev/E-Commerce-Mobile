using Microsoft.AspNetCore.Mvc;
using WebDoDienTu.Models;

namespace WebDoDienTu.Controllers
{
    public class ContactController : Controller
    {
        private ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
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
