using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebDoDienTu.Data;
using WebDoDienTu.Models;
using WebDoDienTu.Service;

namespace WebDoDienTu.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var product = _context.Products.ToList();

            ViewData["Categories"] = _context.Categories.ToList();        
            return View(product);
        } 
    }
}
