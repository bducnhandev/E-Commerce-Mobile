using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;

namespace WebDoDienTu.Controllers
{
    public class ProductReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Thêm đánh giá cho sản phẩm
        [HttpPost]
        public async Task<IActionResult> AddReview(int productId,string name, string email, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = user.Id,
                YourName = name,
                YourEmail = email,
                Rating = rating,
                Comment = comment
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your review has been submitted successfully!";

            return Json(new { success = true, message = "Your review has been submitted successfully!" });
        }
    }
}
