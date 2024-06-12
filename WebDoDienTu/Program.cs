using Microsoft.EntityFrameworkCore;
using WebDoDienTu.Models.Repository;
using WebDoDienTu.Models;
using Microsoft.AspNetCore.Identity;
using WebDoDienTu.Service;
using System.Configuration;
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

        builder.Services.AddAuthentication()
       .AddGoogle(options =>
       {
           IConfigurationSection googleAuthNSection =
           config.GetSection("Authentication:Google");
           options.ClientId = googleAuthNSection["ClientId"];
           options.ClientSecret = googleAuthNSection["ClientSecret"];
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

        app.MapRazorPages();

        app.MapControllerRoute(
            name: "Admin",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");



        app.Run();
    }
}

