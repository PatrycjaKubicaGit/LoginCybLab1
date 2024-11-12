using LoginCybLab1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LoginCybLab1.Models;
using LoginCybLab1.Validator;
using Microsoft.AspNetCore.Identity.UI.Services;
using LoginCybLab1.Areas.Identity.Pages.Account;
using System.Configuration;
using Microsoft.Extensions.Options;

namespace LoginCybLab1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<CybDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            // Dodaj to¿samoœæ z niestandardowym walidatorem has³a
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Pobranie opcji polityki has³a z konfiguracji
                var passwordOptions = builder.Configuration.GetSection("PasswordPolicy").Get<PasswordPolicyViewModel>();
                if (passwordOptions != null)
                {
                    options.Password.RequireDigit = passwordOptions.RequireDigit;
                    options.Password.RequiredLength = passwordOptions.RequiredLength;
                    options.Password.RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
                    options.Password.RequireUppercase = passwordOptions.RequireUppercase;
                    options.Password.RequireLowercase = passwordOptions.RequireLowercase;

                    //MECHANIZM B£ÊDNYCH LOGOWAÑ
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;
                }
            })
          .AddEntityFrameworkStores<CybDbContext>()
          .AddDefaultTokenProviders()
          .AddPasswordValidator<CustomPasswordValidator>()
          .AddSignInManager<SignInManager<ApplicationUser>>()
          .AddDefaultUI();


            builder.Services.AddSingleton<IEmailSender, NullEmailSender>(); 
            // Dodaj Razor Pages
            builder.Services.AddRazorPages();

            //MECHANIZM SESJI
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15); 
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
              
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15); 
                options.SlidingExpiration = true; 
            });

            builder.Services.AddScoped<IUserActivityService, UserActivityService>();

            //google reCaptcha dla zmiany has³a
            builder.Services.AddHttpClient();
            builder.Services.Configure<ReCaptchaSettings>(builder.Configuration.GetSection("GoogleReCaptcha"));



            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();
            

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new[] { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string email = "admin1@admin.com";
                string password = "Admin123@";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser();
                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;
                    user.MustChangePassword = false;
                    user.LockoutEnabled = true;
                    user.AccessFailedCount = 5; 
                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }


            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string email = "admin@admin.com";
                string password = "Admin123@";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser();
                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;
                    user.AccessFailedCount = 5;

                    user.MustChangePassword = false;
                    user.LockoutEnabled = true;

                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string email = "test@test.com";
                string password = "Admin123@";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser();
                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;
                    user.MustChangePassword = false;
                user.LockoutEnabled = true;
                    user.AccessFailedCount = 5;



                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "User");
                }
            }
            app.Run();
        }
    }
}