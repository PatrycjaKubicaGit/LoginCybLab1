// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using LoginCybLab1.Data;
using LoginCybLab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace LoginCybLab1.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly CybDbContext _context;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            CybDbContext context)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Znajdź użytkownika na podstawie emaila
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Nie ujawniaj, że użytkownik nie istnieje lub nie potwierdził emaila
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Wygeneruj token resetu hasła
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Wyślij email z linkiem do resetu hasła
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                // Zapisz nowe hasło do historii, po zresetowaniu hasła
                var newPassword = "NEW_PASSWORD_HERE";  // To powinno być nowe hasło użytkownika, np. Input.Password
                var hashedNewPassword = _userManager.PasswordHasher.HashPassword(user, newPassword);

                // Sprawdź, czy nowe hasło różni się od ostatnich 12
                var previousPasswords = _context.PasswordHistories
                    .Where(ph => ph.UserId == user.Id)
                    .OrderByDescending(ph => ph.DateChanged)
                    .Take(12)
                    .Select(ph => ph.HashedPassword)
                    .ToList();

                foreach (var oldPassword in previousPasswords)
                {
                    if (_userManager.PasswordHasher.VerifyHashedPassword(user, oldPassword, newPassword) == PasswordVerificationResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "New password must be different from the last 12 passwords.");
                        return Page();
                    }
                }

                // Zapisz nowe hasło do historii
                var passwordHistory = new PasswordHistory
                {
                    UserId = user.Id,
                    HashedPassword = hashedNewPassword,
                    DateChanged = DateTime.UtcNow
                };

                _context.PasswordHistories.Add(passwordHistory);

                // Usuń hasła starsze niż 12, jeśli istnieje więcej
                var userPasswordHistory = _context.PasswordHistories
                    .Where(ph => ph.UserId == user.Id)
                    .OrderByDescending(ph => ph.DateChanged)
                    .ToList();

                if (userPasswordHistory.Count > 12)
                {
                    _context.PasswordHistories.RemoveRange(userPasswordHistory.Skip(12));
                }

                await _context.SaveChangesAsync();

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
