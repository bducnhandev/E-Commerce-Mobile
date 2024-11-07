using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebDoDienTu.Data;
using WebDoDienTu.Extensions;
using WebDoDienTu.Models;
using WebDoDienTu.Models.Repository;
using WebDoDienTu.Models.ViewModels;
using WebDoDienTu.Service.MailKit;
using WebDoDienTu.Service.MomoPayment;
using WebDoDienTu.Service.PayPal;
using WebDoDienTu.Service.VNPayPayment;

namespace WebDoDienTu.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private readonly IMomoPaymentService _momoPaymentService;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private static Promotion ?promotion;
        private static Order orderTemp = new Order();
        private readonly IPayPalPaymentService _payPalPaymentService;

        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository, IVnPayService vnPayService, IMomoPaymentService momoPaymentService, 
            IConfiguration configuration, IEmailSender emailSender, IPayPalPaymentService payPalPaymentService)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _momoPaymentService = momoPaymentService;
            _configuration = configuration;
            _emailSender = emailSender;
            _payPalPaymentService = payPalPaymentService;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
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

        public IActionResult Checkout(string voucherCode)
        {
            if (!string.IsNullOrEmpty(voucherCode))
            {
                string code = voucherCode.ToUpper();
                promotion = _context.Promotions.FirstOrDefault(v => v.Code.ToUpper() == code);
            }

            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string payment)
        {
            if(ModelState.IsValid)
            {
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

                if (cart == null || !cart.Items.Any())
                {
                    TempData["EmptyCartMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                    return RedirectToAction("Index");
                }

                if (payment == "Thanh toán MoMo")
                {
                    var user = await _userManager.GetUserAsync(User);
                    order.UserId = user.Id;
                    order.OrderDate = DateTime.UtcNow;

                    // Tính toán giá trị đơn hàng
                    var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                    var discount = promotion != null && promotion.IsPercentage
                                    ? originPrice * promotion.DiscountPercentage / 100
                                    : promotion?.DiscountAmount ?? 0;

                    order.TotalPrice = originPrice - discount;
                    order.OrderDetails = cart.Items.Select(i => new OrderDetail
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList();

                    // Tạo yêu cầu thanh toán cho MoMo
                    var momoModel = new MomoPaymentRequestModel
                    {
                        OrderId = DateTime.UtcNow.Ticks.ToString(),
                        OrderInfo = $"{order.FirstName} {order.LastName} {order.Phone}",
                        Amount = (double)(originPrice - discount),
                        Signature = ""
                    };

                    var paymentUrl = await _momoPaymentService.CreatePaymentUrl(momoModel);

                    // Chuyển hướng đến URL thanh toán MoMo
                    return Redirect(paymentUrl);
                }
                else if (payment == "Thanh toán PayPal")
                {
                    // Calculate the total price for the order
                    var totalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

                    // Create PayPal payment
                    var returnUrl = Url.Action("PaymentSuccess", "Cart", null, Request.Scheme);
                    var cancelUrl = Url.Action("PaymentCancel", "Cart", null, Request.Scheme);
                    var paymentResponse = _payPalPaymentService.CreatePayment(totalAmount, returnUrl, cancelUrl);

                    // Redirect the user to PayPal for approval
                    var approvalUrl = paymentResponse.links.FirstOrDefault(link => link.rel == "approval_url")?.href;
                    return Redirect(approvalUrl);
                }

                else if (payment == "Thanh toán VNPay")
                {
                    orderTemp = order;
                    if(promotion != null)
                    {
                        var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                        var discount = promotion.IsPercentage ?
                                        originPrice * promotion.DiscountPercentage / 100 :
                                        promotion.DiscountAmount;
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
                    if (promotion != null)
                    {                       
                        order.UserId = user.Id;
                        order.OrderDate = DateTime.UtcNow;
                        var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                        var discount = promotion.IsPercentage ?
                                        originPrice * promotion.DiscountPercentage / 100 :
                                        promotion.DiscountAmount;
                        order.TotalPrice = originPrice - discount;
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
                        order.OrderDetails = cart.Items.Select(i => new OrderDetail
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList();
                    }           
                }
                    
                _context.Orders.Add(order);
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
                decimal discount = 0;
                if (promotion != null)
                {
                    discount = promotion.IsPercentage ? originPrice * promotion.DiscountPercentage / 100 : promotion.DiscountAmount;
                }
                ordervnPay.TotalPrice = originPrice - discount;
                ordervnPay.FirstName = orderTemp.FirstName;
                ordervnPay.LastName = orderTemp.LastName;
                ordervnPay.Phone = orderTemp.Phone;
                ordervnPay.Email = orderTemp.Email;
                ordervnPay.Address = orderTemp.Address;
                ordervnPay.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();

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

        public async Task<IActionResult> PreOrder(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null || !product.IsPreOrder || (product.ReleaseDate.HasValue && product.ReleaseDate.Value <= DateTime.UtcNow))
            {
                TempData["Message"] = "Sản phẩm không khả dụng để đặt trước.";
                return RedirectToAction("Index");
            }

            var viewModel = new PreOrderViewModel
            {
                ProductId = productId,
                Quantity = quantity,
                ProductPrice = product.Price,
            };
            ViewBag.Product = product;
            return View("PreOrderForm", viewModel); 
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPreOrder(PreOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.ProductId);
                var totalPrice = product.Price * model.Quantity;

                var minDeposit = totalPrice / 2;
                if (model.DepositAmount < minDeposit)
                {
                    ModelState.AddModelError(nameof(model.DepositAmount), "Giá trị cọc phải lớn hơn hoặc bằng " + minDeposit.ToString("N0") + " VND.");
                    return View("PreOrderForm", model);
                }
                var order = new Order
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.PreOrder,
                    TotalPrice = product.Price * model.Quantity,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Address = model.Address,
                    DepositAmount = model.DepositAmount,
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail
                        {
                            ProductId = model.ProductId,
                            Quantity = model.Quantity,
                            Price = product.Price
                        }
                    }
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Gửi email thông báo
                var userEmail = model.Email;
                var subject = "Xác nhận đơn hàng đặt trước";
                var message = $"Cảm ơn bạn đã đặt hàng trước sản phẩm: {product.ProductName}. " +
                              $"Tổng giá trị đơn hàng: {order.TotalPrice.ToString("N0")} VND." +
                              $"Giá trị cọc: {model.DepositAmount.ToString("N0")} VND." + 
                              $"Số tiền còn lại: {(order.TotalPrice - model.DepositAmount).ToString("N0")} VND.";

                await _emailSender.SendEmailAsync(userEmail, subject, message);

                TempData["Message"] = "Đặt hàng trước thành công!";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

            return View("PreOrderForm", model);
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        private int GetProgressPercentage(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => 10,
                OrderStatus.PreOrder => 20,
                OrderStatus.Processing => 40,
                OrderStatus.Shipped => 60,
                OrderStatus.Delivered => 80,
                OrderStatus.Completed => 100,
                OrderStatus.Cancelled => 0,
                _ => 0,
            };
        }

        public IActionResult OrderDetails(int orderId)
        {
            var order = _context.Orders.Include(o => o.ApplicationUser).FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.ProgressPercentage = GetProgressPercentage(order.Status);
            return View(order);
        }

        public IActionResult PaymentSuccess(string paymentId, string PayerID)
        {
            var payment = _payPalPaymentService.ExecutePayment(paymentId, PayerID);

            if (payment.state.ToLower() == "approved")
            {
                TempData["Message"] = "Thanh toán PayPal thành công!";
                return View("OrderCompleted");
            }

            TempData["Message"] = "Lỗi thanh toán PayPal";
            return RedirectToAction("PaymentFail");
        }

        public IActionResult PaymentCancel()
        {
            TempData["Message"] = "Thanh toán bị hủy.";
            return RedirectToAction("Index");
        }
    }
}
