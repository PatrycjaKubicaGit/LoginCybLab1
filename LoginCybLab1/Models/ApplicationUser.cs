using Microsoft.AspNetCore.Identity;

namespace LoginCybLab1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool? MustChangePassword { get; set; }
        public DateTime? PasswordExpirationDate { get; set; }
        public int? X { get; set; }
    }
}

