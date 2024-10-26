using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LoginCybLab1.Views.ViewModels;
using LoginCybLab1.Models;

namespace LoginCybLab1.Data
{
    public class CybDbContext : IdentityDbContext
    {
        public CybDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<LoginCybLab1.Views.ViewModels.UserViewModel> UserViewModel { get; set; } = default!;
        public DbSet<PasswordHistory> PasswordHistory { get; set; }

        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    }
}
