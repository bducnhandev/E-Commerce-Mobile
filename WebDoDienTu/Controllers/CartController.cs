using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WebDoDienTu.Data;
using WebDoDienTu.Extensions;
using WebDoDienTu.Models;
using WebDoDienTu.Models.ViewModels;
using WebDoDienTu.Repository;
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

            if (product.StockQuantity <= 0)
            {
                TempData["Error"] = $"Sản phẩm {product.ProductName} không đủ hàng để đặt.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + quantity;

                if (newQuantity > 3)
                {
                    TempData["Error"] = $"Mỗi sản phẩm chỉ được phép đặt tối đa 3.";
                    return RedirectToAction("Index");
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                if (quantity > 3) quantity = 3;

                var cartItem = new CartItem
                {
                    ProductId = productId,
                    NameProduct = product.ProductName,
                    Image = product.ImageUrl ?? string.Empty,
                    Price = product.Price,
                    Quantity = quantity
                };

                cart.AddItem(cartItem);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);

                return Json(cart);
            }

            return BadRequest("Sản phẩm không tồn tại trong giỏ hàng.");
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

        //[HttpPost]
        //public async Task<IActionResult> Checkout(Order order, string payment)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

        //        if (cart == null || !cart.Items.Any())
        //        {
        //            TempData["EmptyCartMessage"] = "Giỏ hàng của bạn hiện đang trống.";
        //            return RedirectToAction("Index");
        //        }

        //        if (payment == "Thanh toán MoMo")
        //        {
        //            var user = await _userManager.GetUserAsync(User);
        //            order.UserId = user.Id;
        //            order.OrderDate = DateTime.UtcNow;

        //            // Tính toán giá trị đơn hàng
        //            var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        //            var discount = promotion != null && promotion.IsPercentage
        //                            ? originPrice * promotion.DiscountPercentage / 100
        //                            : promotion?.DiscountAmount ?? 0;

        //            order.TotalPrice = originPrice - discount;
        //            order.OrderDetails = cart.Items.Select(i => new OrderDetail
        //            {
        //                ProductId = i.ProductId,
        //                Quantity = i.Quantity,
        //                Price = i.Price
        //            }).ToList();

        //            // Tạo yêu cầu thanh toán cho MoMo
        //            var momoModel = new MomoPaymentRequestModel
        //            {
        //                OrderId = DateTime.UtcNow.Ticks.ToString(),
        //                OrderInfo = $"{order.FirstName} {order.LastName} {order.Phone}",
        //                Amount = (double)(originPrice - discount),
        //                Signature = ""
        //            };

        //            var paymentUrl = await _momoPaymentService.CreatePaymentUrl(momoModel);

        //            // Chuyển hướng đến URL thanh toán MoMo
        //            return Redirect(paymentUrl);
        //        }
        //        else if (payment == "Thanh toán PayPal")
        //        {
        //            // Calculate the total price for the order
        //            var totalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

        //            // Create PayPal payment
        //            var returnUrl = Url.Action("PaymentSuccess", "Cart", null, Request.Scheme);
        //            var cancelUrl = Url.Action("PaymentCancel", "Cart", null, Request.Scheme);
        //            var paymentResponse = _payPalPaymentService.CreatePayment(totalAmount, returnUrl, cancelUrl);

        //            // Redirect the user to PayPal for approval
        //            var approvalUrl = paymentResponse.links.FirstOrDefault(link => link.rel == "approval_url")?.href;
        //            return Redirect(approvalUrl);
        //        }

        //        else if (payment == "Thanh toán VNPay")
        //        {
        //            orderTemp = order;
        //            if(promotion != null)
        //            {
        //                var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        //                var discount = promotion.IsPercentage ?
        //                                originPrice * promotion.DiscountPercentage / 100 :
        //                                promotion.DiscountAmount;
        //                var vnPayModel = new VnPaymentRequestModel
        //                {
        //                    Amount = (double)originPrice - (double)discount,
        //                    CreatedDate = DateTime.Now,
        //                    Description = $"{order.FirstName} {order.LastName} {order.Phone}",
        //                    FullName = $"{order.FirstName} {order.LastName}",
        //                    OrderId = new Random().Next(1000, 100000)
        //                };
        //                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        //            }
        //            else
        //            {
        //                var vnPayModel = new VnPaymentRequestModel
        //                {
        //                    Amount = (double)cart.Items.Sum(i => i.Price * i.Quantity),
        //                    CreatedDate = DateTime.Now,
        //                    Description = $"{order.FirstName} {order.LastName} {order.Phone}",
        //                    FullName = $"{order.FirstName} {order.LastName}",
        //                    OrderId = new Random().Next(1000, 100000)
        //                };
        //                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        //            }
        //        }
        //        else
        //        {
        //            var user = await _userManager.GetUserAsync(User);
        //            if (promotion != null)
        //            {                       
        //                order.UserId = user.Id;
        //                order.OrderDate = DateTime.UtcNow;
        //                var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        //                var discount = promotion.IsPercentage ?
        //                                originPrice * promotion.DiscountPercentage / 100 :
        //                                promotion.DiscountAmount;
        //                order.TotalPrice = originPrice - discount;
        //                order.OrderDetails = cart.Items.Select(i => new OrderDetail
        //                {
        //                    ProductId = i.ProductId,
        //                    Quantity = i.Quantity,
        //                    Price = i.Price
        //                }).ToList();
        //            }
        //            else
        //            {
        //                order.UserId = user.Id;
        //                order.OrderDate = DateTime.UtcNow;
        //                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        //                order.OrderDetails = cart.Items.Select(i => new OrderDetail
        //                {
        //                    ProductId = i.ProductId,
        //                    Quantity = i.Quantity,
        //                    Price = i.Price
        //                }).ToList();
        //            }           
        //        }

        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();
        //        HttpContext.Session.Remove("Cart");
        //        TempData["Message"] = $"Thanh toán thành công";
        //        return View("OrderCompleted", order.Id);             

        //    }
        //    TempData["ModelState"] = "Vui lòng điền đầy đủ thông tin.";
        //    return View(order);
        //}

        //public async Task<IActionResult> PaymentCallBack()
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var response = _vnPayService.PaymentExecute(Request.Query);
        //        var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

        //        if (response == null || response.VnPayResponseCode != "00")
        //        {
        //            TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
        //            return RedirectToAction("PaymentFail");
        //        }
        //        Order ordervnPay = new Order();
        //        var user = await _userManager.GetUserAsync(User);
        //        ordervnPay.UserId = user.Id;
        //        ordervnPay.OrderDate = DateTime.UtcNow;
        //        var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
        //        decimal discount = 0;
        //        if (promotion != null)
        //        {
        //            discount = promotion.IsPercentage ? originPrice * promotion.DiscountPercentage / 100 : promotion.DiscountAmount;
        //        }
        //        ordervnPay.TotalPrice = originPrice - discount;
        //        ordervnPay.FirstName = orderTemp.FirstName;
        //        ordervnPay.LastName = orderTemp.LastName;
        //        ordervnPay.Phone = orderTemp.Phone;
        //        ordervnPay.Email = orderTemp.Email;
        //        ordervnPay.Address = orderTemp.Address;
        //        ordervnPay.OrderDetails = cart.Items.Select(i => new OrderDetail
        //        {
        //            ProductId = i.ProductId,
        //            Quantity = i.Quantity,
        //            Price = i.Price
        //        }).ToList();

        //        _context.Orders.Add(ordervnPay);
        //        await _context.SaveChangesAsync();
        //        HttpContext.Session.Remove("Cart");

        //        TempData["Message"] = $"Thanh toán VNPay thành công";
        //        return View("OrderCompleted");
        //    }
        //    TempData["Message"] = "Lỗi thanh toán VN Pay";
        //    return RedirectToAction("PaymentFail");
        //}

        private async Task<IActionResult> ProcessPaymentResult(object response, string paymentType)
        {
            if (response == null)
            {
                TempData["Message"] = "Lỗi thanh toán.";
                return RedirectToAction("PaymentFail");
            }

            // Kiểm tra kết quả thanh toán dựa trên loại payment
            bool isPaymentSuccess = false;
            string paymentMessage = "";

            var vnPayResponse = response as VnPaymentResponseModel;
            var payPalResponse = response as PayPal.Api.Payment;

            switch (paymentType.ToLower())
            {
                case "vnpay":
                    if (vnPayResponse != null && vnPayResponse.VnPayResponseCode == "00")
                    {
                        isPaymentSuccess = true;
                        paymentMessage = "Thanh toán VNPay thành công!";
                    }
                    break;

                case "paypal":
                    if (payPalResponse != null && payPalResponse.state.ToLower() == "approved")
                    {
                        isPaymentSuccess = true;
                        paymentMessage = "Thanh toán PayPal thành công!";
                    }
                    break;
            }

            if (isPaymentSuccess)
            {
                var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
                if (paymentType.ToLower() == "vnpay")
                {
                    var order = await CreateVnPayOrder(cart, vnPayResponse);
                    order.Status = OrderStatus.Processing;
                    _context.Orders.Add(order);

                    foreach (var item in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    await _context.SaveChangesAsync();

                    BackgroundJob.Enqueue(() => SendOrderConfirmationEmail(order));
                }
                else if (paymentType.ToLower() == "paypal")
                {
                    var order = await CreatePayPalOrder(cart);
                    order.Status = OrderStatus.Processing;
                    _context.Orders.Add(order);

                    foreach (var item in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    await _context.SaveChangesAsync();            

                    BackgroundJob.Enqueue(() => SendOrderConfirmationEmail(order));
                }

                HttpContext.Session.Remove("Cart");

                TempData["Message"] = paymentMessage;
                return View("OrderCompleted");
            }
            else
            {
                TempData["Message"] = $"Lỗi thanh toán {paymentType.ToUpper()}";
                return RedirectToAction("PaymentFail");
            }
        }

        public async Task SendOrderConfirmationEmail(Order order)
        {
            var o =  _context.Orders.Include(o => o.OrderDetails).ThenInclude(p => p.Product).FirstOrDefault(o => o.Id == order.Id);

            // Load the email template
            var emailTemplatePath = "Templates/OrderConfirm/OrderConfirmationTemplate.html";
            var emailTemplate = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

            // Replace placeholders with actual data
            emailTemplate = emailTemplate.Replace("{{UserName}}", o.FirstName + " " + o.LastName)
                                         .Replace("{{OrderId}}", o.Id.ToString())
                                         .Replace("{{OrderDate}}", o.OrderDate.ToString("MM/dd/yyyy"))
                                         .Replace("{{TotalAmount}}", o.TotalPrice.ToString("#,##0").Replace(",", "."));

            // Construct item list HTML
            var itemsHtml = "";
            foreach (var item in o.OrderDetails)
            {
                itemsHtml += $@"
                                <tr>
                                    <td><img src='{item.Product.ImageUrl}' alt='{item.Product.ProductName}' style='width:50px; height:auto; border-radius:4px;' /></td>
                                    <td>{item.Product.ProductName}</td>
                                    <td>{item.Quantity}</td>
                                    <td>{item.Price:#,##0}₫</td>
                                </tr>";
            }
            emailTemplate = emailTemplate.Replace("{{Items}}", itemsHtml);

            // Send email
            var subject = "Order Confirmation";
            await _emailSender.SendEmailAsync(order.Email, subject, emailTemplate);
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

                BackgroundJob.Enqueue(() => SendConfirmationEmail(order, product, model.Email));
                if (product.ReleaseDate.HasValue && product.ReleaseDate.Value > DateTime.UtcNow)
                {
                    BackgroundJob.Schedule(() => SendLaunchNotification(order, product, model.Email), product.ReleaseDate.Value);
                }

                TempData["Message"] = "Đặt hàng trước thành công!";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }

            return View("PreOrderForm", model);
        }

        public async Task SendConfirmationEmail(Order order, Product product, string userEmail)
        {
            var o = _context.Orders.Include(o => o.OrderDetails).ThenInclude(p => p.Product).FirstOrDefault(o => o.Id == order.Id);

            var subject = "Pre-Order Confirmation";

            // Load the email template
            var emailTemplatePath = "Templates/OrderConfirm/PreOrderConfirmationTemplate.html";
            var emailTemplate = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

            // Replace placeholders with actual data
            emailTemplate = emailTemplate.Replace("{{UserName}}", o.FirstName + " " + o.LastName)
                                         .Replace("{{OrderId}}", o.Id.ToString())
                                         .Replace("{{OrderDate}}", o.OrderDate.ToString("MM/dd/yyyy"))
                                         .Replace("{{TotalAmount}}", o.TotalPrice.ToString("#,##0").Replace(",", "."))
                                         .Replace("{{Pay}}", o.DepositAmount.ToString("#,##0").Replace(",", "."))
                                         .Replace("{{Rest}}", (o.TotalPrice - o.DepositAmount).ToString("#,##0").Replace(",", "."))
                                         .Replace("{{Message}}", "Chúng tôi sẽ thông báo lại khi có hàng");

            // Construct item list HTML
            var itemsHtml = "";
            foreach (var item in o.OrderDetails)
            {
                itemsHtml += $@"
                                <tr>
                                    <td><img src='{item.Product.ImageUrl}' alt='{item.Product.ProductName}' style='width:50px; height:auto; border-radius:4px;' /></td>
                                    <td>{item.Product.ProductName}</td>
                                    <td>{item.Quantity}</td>
                                    <td>{item.Price:#,##0}₫</td>
                                </tr>";
            }
            emailTemplate = emailTemplate.Replace("{{Items}}", itemsHtml);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore 
            };
            var orderJson = JsonConvert.SerializeObject(order, settings);

            await _emailSender.SendEmailAsync(userEmail, subject, emailTemplate);
        }

        public async Task SendLaunchNotification(Order order, Product product, string userEmail)
        {
            var o = _context.Orders.Include(o => o.OrderDetails).ThenInclude(p => p.Product).FirstOrDefault(o => o.Id == order.Id);

            var subject = "Pre-Order Confirmation";

            // Load the email template
            var emailTemplatePath = "Templates/OrderConfirm/PreOrderConfirmationTemplate.html";
            var emailTemplate = await System.IO.File.ReadAllTextAsync(emailTemplatePath);

            // Replace placeholders with actual data
            emailTemplate = emailTemplate.Replace("{{UserName}}", o.FirstName + " " + o.LastName)
                                         .Replace("{{OrderId}}", o.Id.ToString())
                                         .Replace("{{OrderDate}}", o.OrderDate.ToString("MM/dd/yyyy"))
                                         .Replace("{{TotalAmount}}", o.TotalPrice.ToString("#,##0").Replace(",", "."))
                                         .Replace("{{Pay}}", o.DepositAmount.ToString("#,##0").Replace(",", "."))
                                         .Replace("{{Rest}}", (o.TotalPrice - o.DepositAmount).ToString("#,##0").Replace(",", "."))
                                         .Replace("{{Message}}", "Sản phẩm đã bạn đặt hiện đã có hàng. Cảm ơn vì đã quan tâm và chờ đợi");

            // Construct item list HTML
            var itemsHtml = "";
            foreach (var item in o.OrderDetails)
            {
                itemsHtml += $@"
                                <tr>
                                    <td><img src='{item.Product.ImageUrl}' alt='{item.Product.ProductName}' style='width:50px; height:auto; border-radius:4px;' /></td>
                                    <td>{item.Product.ProductName}</td>
                                    <td>{item.Quantity}</td>
                                    <td>{item.Price:#,##0}₫</td>
                                </tr>";
            }
            emailTemplate = emailTemplate.Replace("{{Items}}", itemsHtml);

            // Gửi email thông báo ra mắt qua email sender
            await _emailSender.SendEmailAsync(userEmail, subject, emailTemplate);
        }

        [HttpGet]
        public async Task<IActionResult> PayRemainingBalance(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails)
                                             .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.PreOrder);
            if (order == null || order.DepositAmount == 0)
            {
                TempData["Message"] = "Không tìm thấy đơn hàng cần thanh toán.";
                return RedirectToAction("Index");
            }

            var remainingAmount = order.TotalPrice - order.DepositAmount;
            ViewBag.RemainingAmount = remainingAmount;

            return View(order); 
        }

        [HttpPost]
        public async Task<IActionResult> ProcessRemainingPayment(int orderId, double remainingAmount, string paymentMethod)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails)
                                             .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.PreOrder);
            if (order == null)
            {
                TempData["Message"] = "Không tìm thấy đơn hàng cần thanh toán.";
                return RedirectToAction("Index");
            }

            if (paymentMethod == "MoMo")
            {
                var momoModel = new MomoPaymentRequestModel
                {
                    OrderId = order.Id.ToString(),
                    OrderInfo = $"Thanh toán số tiền còn lại cho đơn hàng {order.Id}",
                    Amount = remainingAmount,
                    Signature = "" // Generate signature here
                };

                var paymentUrl = await _momoPaymentService.CreatePaymentUrl(momoModel);
                return Redirect(paymentUrl);
            }
            else if (paymentMethod == "PayPal")
            {
                // PayPal payment logic
                var returnUrl = Url.Action("PaymentSuccess", "Cart", null, Request.Scheme);
                var cancelUrl = Url.Action("PaymentCancel", "Cart", null, Request.Scheme);
                var paymentResponse = _payPalPaymentService.CreatePayment((decimal)remainingAmount, returnUrl, cancelUrl);
                var approvalUrl = paymentResponse.links.FirstOrDefault(link => link.rel == "approval_url")?.href;
                return Redirect(approvalUrl);
            }
            else if (paymentMethod == "VNPay")
            {
                // VNPay payment logic
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = remainingAmount,
                    CreatedDate = DateTime.Now,
                    Description = $"Thanh toán số tiền còn lại cho đơn hàng {order.Id}",
                    FullName = $"{order.FirstName} {order.LastName}",
                    OrderId = order.Id
                };
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            TempData["Message"] = "Phương thức thanh toán không hợp lệ.";
            return RedirectToAction("Index");
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
            var order = _context.Orders
                        .Include(o => o.ApplicationUser)
                        .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                        .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            ViewBag.ProgressPercentage = GetProgressPercentage(order.Status);
            return View(order);
        }

        public async Task<IActionResult> PaymentSuccess(string paymentId, string PayerID)
        {
            var payment = _payPalPaymentService.ExecutePayment(paymentId, PayerID);

            //if (payment.state.ToLower() == "approved")
            //{
            //    TempData["Message"] = "Thanh toán PayPal thành công!";
            //    return View("OrderCompleted");
            //}

            //TempData["Message"] = "Lỗi thanh toán PayPal";
            //return RedirectToAction("PaymentFail");


            return await ProcessPaymentResult(payment, "paypal");
        }

        public IActionResult PaymentFail()
        {
            TempData["Message"] = "Thanh toán không thành công. Vui lòng thử lại.";
            return View();
        }

        public IActionResult PaymentCancel()
        {
            TempData["Message"] = "Thanh toán bị hủy.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string payment)
        {
            if (!ModelState.IsValid)
            {
                TempData["ModelState"] = "Vui lòng điền đầy đủ thông tin.";
                return View(order);
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            if (cart == null || !cart.Items.Any())
            {
                TempData["EmptyCartMessage"] = "Giỏ hàng của bạn hiện đang trống.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;

            // Tính toán giá trị đơn hàng
            var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            var discount = CalculateDiscount(originPrice);          
            order.OrderDetails = CreateOrderDetails(cart);
            orderTemp = order;

            // Xử lý thanh toán
            switch (payment)
            {
                case "Thanh toán MoMo":
                    return await ProcessMomoPayment(order, originPrice, discount);

                case "Thanh toán PayPal":
                    return await ProcessPayPalPayment(originPrice, discount);

                case "Thanh toán VNPay":
                    return await ProcessVnPayPayment(order, originPrice, discount);

                default:
                    return await ProcessCashOnDelivery(order, originPrice, discount);
            }
        }

        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return await ProcessPaymentResult(response, "vnpay");
        }

        private async Task<Order> CreateVnPayOrder(ShoppingCart cart, VnPaymentResponseModel response)
        {
            var user = await _userManager.GetUserAsync(User);
            var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            var discount = CalculateDiscount(originPrice);

            return new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalPrice = originPrice - discount,
                FirstName = orderTemp.FirstName,
                LastName = orderTemp.LastName,
                Phone = orderTemp.Phone,
                Email = orderTemp.Email,
                Address = orderTemp.Address,
                OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };
        }

        private async Task<Order> CreatePayPalOrder(ShoppingCart cart)
        {
            var user = await _userManager.GetUserAsync(User);
            var originPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            var discount = CalculateDiscount(originPrice);

            return new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                TotalPrice = originPrice - discount,
                FirstName = orderTemp.FirstName,
                LastName = orderTemp.LastName,
                Phone = orderTemp.Phone,
                Email = orderTemp.Email,
                Address = orderTemp.Address,
                OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };
        }

        private async Task<IActionResult> ProcessCashOnDelivery(Order order, decimal originPrice, decimal discount)
        {
            order.TotalPrice = originPrice - discount;
            order.Status = OrderStatus.Processing;
            _context.Orders.Add(order);
            foreach (var item in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");

            BackgroundJob.Enqueue(() => SendOrderConfirmationEmail(order));

            TempData["Message"] = "Thanh toán thành công";
            return View("OrderCompleted", order.Id);
        }

        private async Task<IActionResult> ProcessVnPayPayment(Order order, decimal originPrice, decimal discount)
        {
            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = (double)(originPrice - discount),
                CreatedDate = DateTime.Now,
                Description = $"{order.FirstName} {order.LastName} {order.Phone}",
                FullName = $"{order.FirstName} {order.LastName}",
                OrderId = new Random().Next(1000, 100000)
            };

            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        private async Task<IActionResult> ProcessPayPalPayment(decimal originPrice, decimal discount)
        {
            decimal totalAmount = originPrice - discount;
            decimal exchangeRate = 24275m; // 1 USD = 25,275 VND

            // Chuyển đổi từ VND sang USD
            decimal totalAmountUSD = totalAmount / exchangeRate;
            totalAmountUSD = Math.Round(totalAmountUSD, 2);

            var returnUrl = Url.Action("PaymentSuccess", "Cart", null, Request.Scheme);
            var cancelUrl = Url.Action("PaymentCancel", "Cart", null, Request.Scheme);
            var paymentResponse = _payPalPaymentService.CreatePayment(totalAmountUSD, returnUrl, cancelUrl);

            var approvalUrl = paymentResponse.links.FirstOrDefault(link => link.rel == "approval_url")?.href;
            return Redirect(approvalUrl);
        }

        private List<OrderDetail>? CreateOrderDetails(ShoppingCart cart)
        {
            return cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
        }

        private async Task<IActionResult> ProcessMomoPayment(Order order, decimal originPrice, decimal discount)
        {
            var momoModel = new MomoPaymentRequestModel
            {
                OrderId = DateTime.UtcNow.Ticks.ToString(),
                OrderInfo = $"{order.FirstName} {order.LastName} {order.Phone}",
                Amount = (double)(originPrice - discount),
                Signature = "" // Add signature logic here
            };

            var paymentUrl = await _momoPaymentService.CreatePaymentUrl(momoModel);
            return Redirect(paymentUrl);
        }

        private decimal CalculateDiscount(decimal originPrice)
        {
            if (promotion == null)
                return 0;

            return promotion.IsPercentage ?
                   originPrice * promotion.DiscountPercentage / 100 :
                   promotion.DiscountAmount;
        }
    }
}
