using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;

namespace WebDoDienTu.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var wishList = await _context.WishLists
                                            .Include(w => w.WishListItems)
                                            .FirstOrDefaultAsync(w => w.UserId == user.Id);
            return View(wishList.WishListItems);
        }

        // Thêm sản phẩm vào wishlist
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await _context.WishLists
                                         .Include(w => w.WishListItems)
                                         .FirstOrDefaultAsync(w => w.UserId == user.Id);
            if (wishlist == null)
            {
                wishlist = new WishList { UserId = user.Id };
                _context.WishLists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            if (!wishlist.WishListItems.Any(wi => wi.ProductId == productId))
            {
                var wishlistItem = new WishListItem
                {
                    ProductId = productId,
                    WishListId = wishlist.Id,
                    AddedDate = DateTime.UtcNow
                };
                _context.WishListItem.Add(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
