using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LoginCybLab1.Views.ViewModels;

namespace LoginCybLab1.Data
{
    public class CybDbContext : IdentityDbContext
    {
        public CybDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<LoginCybLab1.Views.ViewModels.UserViewModel> UserViewModel { get; set; } = default!;
    }
}
