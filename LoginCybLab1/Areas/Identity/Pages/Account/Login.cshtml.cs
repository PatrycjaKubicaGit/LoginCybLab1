using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LoginCybLab1.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.ComponentModel.DataAnnotations;

namespace LoginCybLab1.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private static Random _random = new Random();
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        private readonly ILogger<LoginModel> _logger;
        private readonly IUserActivityService _activityService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginModel> logger, IUserActivityService activityService, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _activityService = activityService;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }


            [Display(Name = "Pamiętaj mnie")]
            public bool RememberMe { get; set; }

            [Required]
            [Range(0, int.MaxValue, ErrorMessage = "Błędna odpowiedź CAPTCHA")]
            public int CaptchaResponse { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public IActionResult OnGetGenerateCaptcha()
        {
            int num1 = _random.Next(1, 10);
            int num2 = _random.Next(1, 10);
            int result = num1 + num2;

            HttpContext.Session.SetInt32("CaptchaResult", result);
            string text = $"{num1} + {num2}";


            var bitmap = GenerateImage(200, 100, text);

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }
        }

        private Bitmap GenerateImage(int width, int height, string text)
        {
            var random = new Random();
            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, width, height);

            HatchBrush hatchBrush = new HatchBrush(
                HatchStyle.SmallConfetti,
                Color.LightGray,
                Color.White);
            g.FillRectangle(hatchBrush, rect);

            SizeF size;
            float fontSize = rect.Height + 1;
            Font font;
            do
            {
                fontSize--;
                font = new Font("Arial", fontSize, FontStyle.Bold);
                size = g.MeasureString(text, font);
            } while (size.Width > rect.Width);

            StringFormat format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            GraphicsPath path = new GraphicsPath();
            path.AddString(text, font.FontFamily, (int)font.Style, font.Size, rect, format);
            float v = 4F;
            PointF[] points =
            {
                new PointF(random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                new PointF(rect.Width - random.Next(rect.Width) / v, random.Next(rect.Height) / v),
                new PointF(random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v),
                new PointF(rect.Width - random.Next(rect.Width) / v, rect.Height - random.Next(rect.Height) / v)
            };
            Matrix matrix = new Matrix();
            path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

            hatchBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray);
            g.FillPath(hatchBrush, path);

            int m = Math.Max(rect.Width, rect.Height);
            for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
            {
                int x = random.Next(rect.Width);
                int y = random.Next(rect.Height);
                int w = random.Next(m / 50);
                int h = random.Next(m / 50);
                g.FillEllipse(hatchBrush, x, y, w, h);
            }

            font.Dispose();
            hatchBrush.Dispose();
            g.Dispose();

            return bitmap;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            int userCaptchaAnswer = Input.CaptchaResponse;

            var captchaResult = HttpContext.Session.GetInt32("CaptchaResult");
            if (captchaResult != userCaptchaAnswer)
            {
                ModelState.AddModelError(string.Empty, "Błędna odpowiedź CAPTCHA");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var resultLog = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (resultLog.IsLockedOut)
                {
                    _logger.LogWarning("Konto użytkownika zablokowane.");
                    return RedirectToPage("./Lockout");
                }
                if (resultLog.Succeeded)
                {
                    HttpContext.Session.SetString("LastActivity", DateTime.UtcNow.ToString());
                    await _activityService.LogActivity(Input.Email, "Login", $"Zalogowany {DateTime.UtcNow.ToString()}");


                    var user = _userManager.FindByEmailAsync(Input.Email).Result;
                    if (user != null && user.MustChangePassword == true)
                    {
                        await _activityService.LogActivity(Input.Email, "Change one-set password", $"Zmiana hasła jednorazowego dla użytkownika {user.UserName}.");
                        user.MustChangePassword = false;
                        return LocalRedirect("ForgotPassword");
                    }

                    _logger.LogInformation("User logged in.");
                    await _activityService.LogLogin(Input.Email);

                    return LocalRedirect(returnUrl);
                }
                if (resultLog.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (resultLog.IsLockedOut)
                {
                    var dnsToken = _configuration.GetValue<string>("Tokens:DNS");
                    try
                    {
                        using var client = new HttpClient();
                        await client.GetAsync($"http://{dnsToken}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error triggering DNS token.");
                        _activityService.LogActivity("DNS", "DNS Token", "Error triggering DNS token. " + ex.Message);
                    }

                    _activityService.LogActivity("DNS", "DNS Token", "DNS Token Triggered.");
                    TempData["DNSSuccessMessage"] = "DNS Token Triggered.";

                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    var user = _userManager.FindByEmailAsync(Input.Email).Result;
                    if (user != null && user.MustChangePassword == true)
                    {
                        ModelState.AddModelError(string.Empty, "Use single-use password!");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt (wrong login or password)");
                    }
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        public async Task<IActionResult> TriggerDnsToken()
        {
            var dnsToken = _configuration.GetValue<string>("Tokens:DNS");
            try
            {
                // Wysyłanie zapytania DNS
                using var client = new HttpClient();
                await client.GetAsync($"http://{dnsToken}");


                _activityService.LogActivity("DNS", "DNS Token", "DNS Token Triggered.");
                TempData["DNSSuccessMessage"] = "DNS Token Triggered.";


                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering DNS token.");
                _activityService.LogActivity("DNS", "DNS Token", "Error triggering DNS token. " + ex.Message);
                return StatusCode(500, "Failed to trigger DNS token.");
            }
        }
    }
}
