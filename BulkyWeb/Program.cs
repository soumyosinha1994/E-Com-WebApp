using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using BulkyWeb.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bulky.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Bulky.Models;
using Stripe;
using Bulky.DataAccess.DBInitializer;

namespace BulkyWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddScoped<IDBInitializer, DBInitializer>();

            //configure DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));



            //builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
            //builder.Services.AddDefaultIdentity<ApplicationUser>().AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

    //        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    //        {
    //            options.User.RequireUniqueEmail = true;
    //        })
    //.AddEntityFrameworkStores<ApplicationDbContext>()
    //.AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options => {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            //builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
            builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
            builder.Services.AddScoped<IEmailSender,EmailSender>();

            builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));

            //Add Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromMinutes(100);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication().AddFacebook(option => {
                option.AppId = "757630669867120";
                option.AppSecret = "6a8b2f7abb79ad92dead970b46ec4a15";
            });
            builder.Services.AddAuthentication().AddMicrosoftAccount(option => {
                option.ClientId = "ec4d380d-d631-465d-b473-1e26ee706331";
                option.ClientSecret = "qMW8Q~LlEEZST~SDxDgcEVx_45LJQF2cQ_rEKcSQ";
            });
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

            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            SeedDatabase();
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();

            void SeedDatabase()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
                    dbInitializer.Initialize();
                }
            }
        }

    }
}
