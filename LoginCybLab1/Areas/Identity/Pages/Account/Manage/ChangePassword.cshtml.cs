// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using LoginCybLab1.Models;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace LoginCybLab1.Areas.Identity.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ChangePasswordModel> _logger;
        private readonly IUserActivityService _activityService;
        private readonly IOptions<ReCaptchaSettings> _reCaptchaSettings;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ChangePasswordModel> logger,
            IUserActivityService activityService,
            IOptions<ReCaptchaSettings> reCaptchaSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _activityService = activityService;
            _reCaptchaSettings = reCaptchaSettings;
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
        [TempData]
        public string StatusMessage { get; set; }
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
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
        public async Task<IActionResult> OnGetAsync()
        {
            //captcha
            ViewData["ReCaptchaSiteKey"] = _reCaptchaSettings.Value.SiteKey;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = User.Identity.Name;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            //captcha
            var reCaptchaResponse = Request.Form["g-recaptcha-response"];
            var isReCaptchaValid = await ValidateReCaptchaAsync(reCaptchaResponse);

            if (!isReCaptchaValid)
            {
                ModelState.AddModelError(string.Empty, "Potwierdzenie reCAPTCHA nie powiodło się.");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
            await _activityService.LogPasswordChange(user.UserName);
            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your password has been changed.";
            return RedirectToPage();
        }
        //metoda captcha
        private async Task<bool> ValidateReCaptchaAsync(string reCaptchaResponse)
        {
            var secretKey = _reCaptchaSettings.Value.SecretKey;
            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={reCaptchaResponse}",
                null
            );

            if (!response.IsSuccessStatusCode) return false;

            var jsonString = await response.Content.ReadAsStringAsync();
            var reCaptchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonString);
            return reCaptchaResult?.Success ?? false;
        }

        public class ReCaptchaResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }

    }
}