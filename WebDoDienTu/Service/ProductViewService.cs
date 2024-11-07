using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Data;
using WebDoDienTu.Models;

namespace WebDoDienTu.Service
{
    public class ProductViewService
    {
        private readonly ApplicationDbContext _context;

        public ProductViewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RecordProductViewAsync(string userId, int productId)
        {
            var productView = await _context.ProductViews
                .FirstOrDefaultAsync(pv => pv.UserId == userId && pv.ProductId == productId);

            if (productView != null)
            {
                productView.ViewCount++;
                productView.LastViewedDate = DateTime.Now;
            }
            else
            {
                productView = new ProductView
                {
                    UserId = userId,
                    ProductId = productId,
                    ViewCount = 1,
                    LastViewedDate = DateTime.Now
                };
                _context.ProductViews.Add(productView);
            }

            await _context.SaveChangesAsync();
        }
    }
}
