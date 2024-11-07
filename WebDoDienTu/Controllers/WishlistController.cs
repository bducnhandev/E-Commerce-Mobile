using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Data;
using WebDoDienTu.Extensions;
using WebDoDienTu.Models;
using WebDoDienTu.Models.ViewModels;

namespace WebDoDienTu.Controllers
{
    [Authorize]
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
            var wishList = await _context.WishLists
                                            .Include(w => w.WishListItems)
                                            .FirstOrDefaultAsync(w => w.UserId == user.Id);
            if (wishList == null) {
                TempData["Message"] = "Danh mục yêu thích hiện trống";
                return View("Index");
            }
            WishListViewModel wishListViewModel = new WishListViewModel();
            wishListViewModel.WishListItems = wishList.WishListItems;
            wishListViewModel.Products = await _context.Products.ToListAsync();
            return View(wishListViewModel);
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
                _context.WishListItems.Add(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishList(int productId)
        {
            var item = await _context.WishListItems.FindAsync(productId);
            if (item == null) { return RedirectToAction("Index"); }
            _context.WishListItems.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            var cartItem = new CartItem
            {
                ProductId = productId,
                NameProduct = product.ProductName,
                Image = product.ImageUrl ?? string.Empty,
                Price = product.Price,
                Quantity = quantity
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index", "Cart");
        }
    }
}
