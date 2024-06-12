using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;

namespace WebDoDienTu.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VoucherController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetVoucher(string code)
        {
            var voucher = await _context.Vouchers
                .Where(v => v.Code == code && v.ExpiryDate >= DateTime.Now && v.SoLuong > 0)
                .FirstOrDefaultAsync();

            if (voucher == null)
            {
                return NotFound(new { message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });
            }

            return Ok(voucher);
        }
    }

}
