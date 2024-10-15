using LoginCybLab1.Data;
using Microsoft.AspNetCore.Identity;

namespace LoginCybLab1.Validator;

public class CustomPasswordValidator : IPasswordValidator<IdentityUser>
{
    private readonly CybDbContext _context;

    public CustomPasswordValidator(CybDbContext context)
    {
        _context = context;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string password)
    {
        var previousPasswords = _context.PasswordHistories
            .Where(ph => ph.UserId == user.Id)
            .OrderByDescending(ph => ph.DateChanged)
            .Take(12)
            .Select(ph => ph.HashedPassword)
            .ToList();

        foreach (var oldPassword in previousPasswords)
        {
            if (manager.PasswordHasher.VerifyHashedPassword(user, oldPassword, password) == PasswordVerificationResult.Success)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "New password must be different from the last 12 passwords."
                });
            }
        }

        return IdentityResult.Success;
    }
}

