using LoginCybLab1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace LoginCybLab1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
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

            if (licenseKey == "KOD-LICENCJA") // tutaj potem dodać kod z szyfru
            {
                var user = await _userManager.GetUserAsync(User);
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