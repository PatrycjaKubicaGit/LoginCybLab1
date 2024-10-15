using Microsoft.AspNetCore.Identity;

namespace LoginCybLab1.Models;

public class PasswordHistory
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string HashedPassword { get; set; }
    public DateTime DateChanged { get; set; }

    public virtual IdentityUser User { get; set; }
}

