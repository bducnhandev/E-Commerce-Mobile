using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models;
using Microsoft.AspNetCore.Identity;
using WebDoDienTu.Service;
using OfficeOpenXml;
using WebDoDienTu.Hubs;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using WebDoDienTu.Service.MomoPayment;
using WebDoDienTu.Service.VNPayPayment;
using WebDoDienTu.Service.MailKit;
using WebDoDienTu.Service.Cloudinary;
using WebDoDienTu.Data;
using WebDoDienTu.Service.PayPal;
using Hangfire;
using Hangfire.SqlServer;
using WebDoDienTu.Service.ProductRecommendationService;
using WebDoDienTu.Repository;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        builder.Services.AddSignalR();

        // Configuring Hangfire with SQL Server storage
        builder.Services.AddHangfire(configuration => configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                         .UseSimpleAssemblyNameTypeSerializer()
                         .UseDefaultTypeSerializer()
                         .UseSqlServerStorage(config.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                         {
                             CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                             SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                             QueuePollInterval = TimeSpan.Zero,
                             UseRecommendedIsolationLevel = true,
                             DisableGlobalLocks = true
                         }));

        // Add the processing server as IHostedService
        builder.Services.AddHangfireServer();

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        });

        builder.Services.ConfigureApplicationCookie(option =>
        {
            option.LoginPath = $"/Identity/Account/Login";
            option.LogoutPath = $"/Identity/Account/Logout";
            option.LogoutPath = $"/Identity/Account/AccesDenied";
        });

        builder.Services.AddRazorPages();

        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IProductRepository, EFProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
        builder.Services.AddScoped<IPostCategoryRepository, PostCategoryRepository>();
        builder.Services.AddScoped<IPostRepository, PostRepository>();
        builder.Services.AddScoped<ICommentRepository, CommentRepository>();
        builder.Services.AddSingleton<IVnPayService, VnPayService>();
        builder.Services.AddScoped<IMomoPaymentService, MomoPaymentService>();
        builder.Services.AddScoped<ProductViewService>();
        builder.Services.AddScoped<RecommendationService>();
        builder.Services.AddScoped<ProductRecommendationService>();
        builder.Services.AddScoped<IPayPalPaymentService, PayPalPaymentService>();

        //builder.Services.Configure<AuthMessageSenderOptions>(config.GetSection("MailjetSettings"));
        //builder.Services.AddTransient<IEmailSender, EmailSender>();

        builder.Services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        builder.Services.AddTransient<IEmailSender, EmailSender>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddAuthentication().AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        });

        builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
            facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
            facebookOptions.AccessDeniedPath = "/AccessDeniedPathInfo";
        });

        builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

        builder.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
            var account = new Account(
                config.CloudName,
                config.ApiKey,
                config.ApiSecret
            );
            return new Cloudinary(account);
        });

        // Cấu hình Localization
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // Cấu hình localization cho ứng dụng
        var supportedCultures = new[] { "en-US", "vi-VN" };

        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture("en-US")
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);
        app.UseRequestLocalization(localizationOptions);

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseSession();

        app.UseRouting();

        app.UseAuthorization();

        CreateDefaultAdminUser(app).GetAwaiter().GetResult();

        app.MapRazorPages();

        app.MapControllerRoute(
            name: "Admin",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "adminChat",
            pattern: "Admin/Chat/{action=AdminChat}/{id?}",
            defaults: new { controller = "Chat", action = "AdminChat" });

        app.MapControllerRoute(
            name: "userChat",
            pattern: "User/Chat/{action=UserChat}/{id?}",
            defaults: new { controller = "Chat", action = "UserChat" });

        app.MapHub<ChatHub>("/chathub");
        app.MapHub<PostHub>("/posthub");

        app.UseHangfireDashboard("/hangfire");

        app.Run();
    }

    private static async Task CreateDefaultAdminUser(WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Tạo vai trò Admin nếu chưa tồn tại
            var role = new IdentityRole(SD.Role_Admin);
            var roleExists = await roleManager.RoleExistsAsync(role.Name);
            if (!roleExists)
            {
                await roleManager.CreateAsync(role);
            }

            // Tạo tài khoản Admin nếu chưa tồn tại
            var user = await userManager.FindByEmailAsync("admin@example.com");
            if (user == null)
            {
                user = new ApplicationUser { UserName = "admin@example.com", Email = "admin@example.com" };
                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role.Name);
                    Console.WriteLine("Admin user created successfully.");
                }
                else
                {
                    Console.WriteLine($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
    }
}

