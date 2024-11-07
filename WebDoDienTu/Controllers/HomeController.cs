using Microsoft.AspNetCore.Mvc;
using WebDoDienTu.Data;

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
            var product = _context.Products.ToList();
            ViewData["Categories"] = _context.Categories.ToList();
            return View(product);
        }
    }
}
