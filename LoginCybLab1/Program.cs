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
            builder.Services.AddDbContext<CybDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>().AddEntityFrameworkStores<CybDbContext>();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddEntityFrameworkStores<CybDbContext>();


            var passwordOptions = builder.Configuration.GetSection("PasswordPolicy").Get<PasswordPolicyViewModel>();
            if (passwordOptions != null)
                builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    var passwordOptions = builder.Configuration.GetSection("PasswordPolicy").Get<PasswordPolicyViewModel>();

                    options.Password.RequireDigit = passwordOptions.RequireDigit;
                    options.Password.RequiredLength = passwordOptions.RequiredLength;
                    options.Password.RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric;
                    options.Password.RequireUppercase = passwordOptions.RequireUppercase;
                    options.Password.RequireLowercase = passwordOptions.RequireLowercase;
                })
                .AddEntityFrameworkStores<CybDbContext>()
                .AddDefaultTokenProviders();


            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

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