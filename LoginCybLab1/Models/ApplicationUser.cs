using Microsoft.AspNetCore.Identity;

namespace LoginCybLab1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
