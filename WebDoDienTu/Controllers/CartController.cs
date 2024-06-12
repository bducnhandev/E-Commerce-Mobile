using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Extensions;
using WebDoDienTu.Models;
using WebDoDienTu.Models.Repository;
using WebDoDienTu.Service;

namespace WebDoDienTu.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository, IVnPayService vnPayService)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            var product = await GetProductFromDatabase(productId);
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
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }
        
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        private static Voucher voucher;
        private static Order orderTemp = new Order();

        public IActionResult Checkout(string voucherCode)
        {
            // Xử lý logic của bạn ở đây, ví dụ:
            if (!string.IsNullOrEmpty(voucherCode))
            {
                string code = voucherCode.ToUpper();
                voucher = _context.Vouchers.FirstOrDefault(v => v.Code.ToUpper() == code);
            }

            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string payment = "COD")
        {
            if(ModelState.IsValid)
            {
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

                if (cart == null || !cart.Items.Any())
                {
                    // Xử lý giỏ hàng trống...
                    TempData["EmptyCartMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                    return RedirectToAction("Index");
                }
                
                if (payment == "Thanh toán VNPay")
                {
                    orderTemp = order;
                    if(voucher != null)
                    {
                        var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                        var discount = (originPrice * voucher.Value) / 100;
                        var vnPayModel = new VnPaymentRequestModel
                        {
                            Amount = (double)originPrice - (double)discount,
                            CreatedDate = DateTime.Now,
                            Description = $"{order.FirstName} {order.LastName} {order.Phone}",
                            FullName = $"{order.FirstName} {order.LastName}",
                            OrderId = new Random().Next(1000, 100000)
                        };
                        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                    }
                    else
                    {
                        var vnPayModel = new VnPaymentRequestModel
                        {
                            Amount = (double)cart.Items.Sum(i => i.Price * i.Quantity),
                            CreatedDate = DateTime.Now,
                            Description = $"{order.FirstName} {order.LastName} {order.Phone}",
                            FullName = $"{order.FirstName} {order.LastName}",
                            OrderId = new Random().Next(1000, 100000)
                        };
                        return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                    }
                }
                else
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (voucher != null)
                    {                       
                        order.UserId = user.Id;
                        order.OrderDate = DateTime.UtcNow;
                        var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                        var discount = (originPrice * voucher.Value) / 100;
                        order.TotalPrice = originPrice - discount;
                        order.Status = "Chưa thanh toán";
                        order.VoucherId = voucher.Id;
                        order.OrderDetails = cart.Items.Select(i => new OrderDetail
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList();
                    }
                    else
                    {
                        order.UserId = user.Id;
                        order.OrderDate = DateTime.UtcNow;
                        order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                        order.Status = "Chưa thanh toán";
                        order.OrderDetails = cart.Items.Select(i => new OrderDetail
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList();
                    }           
                }
                    
                _context.Orders.Add(order);
                if (voucher != null) {
                    voucher.SoLuong -= 1;
                    _context.Vouchers.Update(voucher);
                }              
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                TempData["Message"] = $"Thanh toán thành công";
                return View("OrderCompleted", order.Id);             

            }
            TempData["ModelState"] = "Vui lòng điền đầy đủ thông tin.";
            return View(order);
        }

        public async Task<IActionResult> PaymentCallBack()
        {
            if (ModelState.IsValid)
            {
                var response = _vnPayService.PaymentExecute(Request.Query);
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

                if (response == null || response.VnPayResponseCode != "00")
                {
                    TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                    return RedirectToAction("PaymentFail");
                }
                Order ordervnPay = new Order();
                var user = await _userManager.GetUserAsync(User);
                ordervnPay.UserId = user.Id;
                ordervnPay.OrderDate = DateTime.UtcNow;
                var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                var discount = (originPrice * voucher.Value) / 100;
                ordervnPay.TotalPrice = originPrice - discount;
                ordervnPay.FirstName = orderTemp.FirstName;
                ordervnPay.LastName = orderTemp.LastName;
                ordervnPay.Phone = orderTemp.Phone;
                ordervnPay.Email = orderTemp.Email;
                ordervnPay.Address = orderTemp.Address;
                ordervnPay.VoucherId = voucher.Id;
                ordervnPay.Status = "Đã thanh toán";
                ordervnPay.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();

                if (voucher != null)
                {
                    voucher.SoLuong -= 1;
                    _context.Vouchers.Update(voucher);
                }

                _context.Orders.Add(ordervnPay);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");

                TempData["Message"] = $"Thanh toán VNPay thành công";
                return View("OrderCompleted");
            }
            TempData["Message"] = "Lỗi thanh toán VN Pay";
            return RedirectToAction("PaymentFail");
        }

        public IActionResult PaymentFail()
        {
            return View();
        }
    }
}
