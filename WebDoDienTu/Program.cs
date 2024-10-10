using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models.Repository;
using WebDoDienTu.Models;
using Microsoft.AspNetCore.Identity;
using WebDoDienTu.Service;
using Microsoft.AspNetCore.Identity.UI.Services;
using OfficeOpenXml;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddDefaultTokenProviders()
        .AddDefaultUI()
        .AddEntityFrameworkStores<ApplicationDbContext>();

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
        builder.Services.AddSingleton<IVnPayService, VnPayService>();

        builder.Services.Configure<AuthMessageSenderOptions>(config.GetSection("MailjetSettings"));
        builder.Services.AddTransient<IEmailSender, EmailSender>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.AddAuthentication().AddGoogle(options =>
        {
            IConfigurationSection googleAuthNSection = config.GetSection("Authentication:Google");
            options.ClientId = googleAuthNSection["ClientId"];
            options.ClientSecret = googleAuthNSection["ClientSecret"];
        });

        builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
        {
            facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
            facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
            facebookOptions.AccessDeniedPath = "/AccessDeniedPathInfo";
        });

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

