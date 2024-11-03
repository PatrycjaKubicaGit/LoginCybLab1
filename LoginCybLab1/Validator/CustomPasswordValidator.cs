using LoginCybLab1.Data;
using LoginCybLab1.Models;
using Microsoft.AspNetCore.Identity;

namespace LoginCybLab1.Validator;

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    private readonly CybDbContext _context;

    public CustomPasswordValidator(CybDbContext context)
    {
        _context = context;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
    {
     //   if(_context.PasswordHistory == null)
            return IdentityResult.Success;

        //var previousPasswords = _context.PasswordHistory
        //    .Where(ph => ph.UserId == user.Id)
        //    .OrderByDescending(ph => ph.DateChanged)
        //    .Take(12)
        //    .Select(ph => ph.HashedPassword)
        //    .ToList();

        //foreach (var oldPassword in previousPasswords)
        //{
        //    if (manager.PasswordHasher.VerifyHashedPassword(user, oldPassword, password) == PasswordVerificationResult.Success)
        //    {
        //        return IdentityResult.Failed(new IdentityError
        //        {
        //            Description = "New password must be different from the last 12 passwords."
        //        });
        //    }
        //}

        //return IdentityResult.Success;
    }
}

