using LoginCybLab1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using LoginCybLab1.Models;
using LoginCybLab1.Validator;

namespace LoginCybLab1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Konfiguracja bazy danych
            builder.Services.AddDbContext<CybDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            // Dodaj to¿samoœæ z niestandardowym walidatorem has³a
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
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
                }
            })
            .AddRoles<IdentityRole>()
            .AddPasswordValidator<CustomPasswordValidator>()
            .AddEntityFrameworkStores<CybDbContext>()
            .AddDefaultTokenProviders();

            // Dodaj Razor Pages
            builder.Services.AddRazorPages();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

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
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string email = "admin1@admin.com";
                string password = "Admin123@";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new IdentityUser();
                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;

                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }   
            
            
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string email = "admin@admin.com";
                string password = "Admin123@";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new IdentityUser();
                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;

                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string email = "test@test.com";
                string password = "Admin123@";

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new IdentityUser();
                    user.UserName = email;
                    user.Email = email;
                    user.EmailConfirmed = true;

                    await userManager.CreateAsync(user, password);

                    await userManager.AddToRoleAsync(user, "User");
                }
            }
            app.Run();
        }
    }
}