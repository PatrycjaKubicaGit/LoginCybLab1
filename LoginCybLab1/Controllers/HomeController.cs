using LoginCybLab1.Extensions;
using LoginCybLab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Text;

namespace LoginCybLab1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserActivityService _activityService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IUserActivityService activityService, IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _activityService = activityService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.IsDemo = user.IsDemoUser;
            }
            else
            {
                ViewBag.IsDemo = true; 
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ActivateLicense(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                TempData["ErrorMessage"] = "Klucz licencyjny nie może być pusty.";
                return RedirectToAction("Index");
            }


            var user = await _userManager.GetUserAsync(User);

            var normalizedKey = Vigenere.NormalizeInput(_configuration.GetValue<string>("Vinegere:Key"));
            var normalizedText = Vigenere.NormalizeInput(user.NormalizedUserName);
            var enc = Vigenere.Encrypt(normalizedText, normalizedKey);
            await _activityService.LogActivity(User.Identity.Name, "DEMO", enc);
            await _activityService.LogActivity(User.Identity.Name, "DEMO", Vigenere.Decrypt(enc, normalizedKey));

            if (licenseKey == enc) 
            {
                if (user != null)
                {
                    user.IsDemoUser = false;
                    var updateResult = await _userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                       
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        TempData["SuccessMessage"] = "Licencja została pomyślnie aktywowana.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Wystąpił problem podczas aktualizacji użytkownika.";
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Nieprawidłowy klucz licencyjny.";
            }

            return RedirectToAction("Index");
        }


        [Authorize]
        //generowanie pliku do druku
        public IActionResult Print()
        {
            var user = _userManager.GetUserAsync(User).Result;

            if (user?.IsDemoUser == true)
            {
                return Unauthorized(); 
            }

            string content = @"
                @{
                    ViewData[""Title""] = ""Test Drukowania"";
                }

                <div class=""text-center"">
                    <h1 class=""display-4"">Test Drukowania</h1>
                    <p>Tekst do drukowania</a>.</p>
                </div>";

            ViewBag.ContentToPrint = content;
            return View();
        }
        
        


    }
}