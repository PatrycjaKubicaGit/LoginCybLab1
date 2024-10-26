using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoginCybLab1.Data;
using LoginCybLab1.Views.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Configuration;
using LoginCybLab1.Models;

namespace LoginCybLab1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IUserActivityService _activityService;

        public UsersController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, IUserActivityService activityService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _activityService = activityService;
        }


        // GET: Wyświetlenie formularza zarządzania polityką haseł
        [HttpGet]
        public IActionResult ManagePasswordPolicy()
        {
            var passwordOptions = _configuration.GetSection("PasswordPolicy").Get<PasswordPolicyViewModel>();
            return View(passwordOptions);
        }

        // POST: Zapisanie nowych ustawień polityki haseł
        [HttpPost]
        public IActionResult ManagePasswordPolicy(PasswordPolicyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                config["PasswordPolicy:RequireDigit"] = model.RequireDigit.ToString();
                config["PasswordPolicy:RequireLowercase"] = model.RequireLowercase.ToString();
                config["PasswordPolicy:RequireUppercase"] = model.RequireUppercase.ToString();
                config["PasswordPolicy:RequireNonAlphanumeric"] = model.RequireNonAlphanumeric.ToString();
                config["PasswordPolicy:RequiredLength"] = model.RequiredLength.ToString();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // CREATE: GET - Wyświetlenie formularza dodawania nowego użytkownika
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // CREATE: POST - Tworzenie nowego użytkownika z użyciem Identity
        [HttpPost]
        public async Task<IActionResult> Create(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _activityService.LogActivity(User.Identity.Name, "Create User", $"Użytkownik {email} został utworzony.");
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                await _activityService.LogActivity(User.Identity.Name, "Create User", $"Nieudana próba utworzenia użytkownika {email}.");
            }
            return View();
        }

        // UPDATE: GET - Pobranie danych użytkownika do edycji
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _activityService.LogActivity(User.Identity.Name, "Edit User - GET", $"Wyświetlono formularz edycji użytkownika {user.UserName}.");
            return View(user);
        }

        // UPDATE: POST - Zapisanie edytowanych danych użytkownika
        [HttpPost]
        public async Task<IActionResult> Edit(string id, IdentityUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Email = user.Email;
            existingUser.UserName = user.UserName;

            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                await _activityService.LogActivity(User.Identity.Name, "Edit User - POST", $"Użytkownik {user.UserName} został zaktualizowany.");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            await _activityService.LogActivity(User.Identity.Name, "Edit User - POST", $"Nieudana próba edycji użytkownika {user.UserName}.");
            return View(user);
        }

        // DELETE: GET - Potwierdzenie usunięcia użytkownika
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            //await _activityService.LogActivity(User.Identity.Name, "Delete User - GET", $"Wyświetlono potwierdzenie usunięcia użytkownika {user.UserName}.");
            return View(user);
        }

        // DELETE: POST - Usunięcie użytkownika
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _activityService.LogActivity(User.Identity.Name, "Delete User - POST", $"Użytkownik {user.UserName} został usunięty.");
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            await _activityService.LogActivity(User.Identity.Name, "Delete User - POST", $"Nieudana próba usunięcia użytkownika {user.UserName}.");
            return View(user);
        }

        // BLOKOWANIE: Blokowanie użytkownika
        public async Task<IActionResult> Lock(string id, int days)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(days);
            user.LockoutEnabled = true;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _activityService.LogActivity(User.Identity.Name, "Lock User", $"Użytkownik {user.UserName} został zablokowany na {days} dni.");
                return RedirectToAction("Index");
            }

            await _activityService.LogActivity(User.Identity.Name, "Lock User", $"Nieudana próba zablokowania użytkownika {user.UserName}.");
            return View("Index");
        }

        // ODBLOKOWANIE: Odblokowanie użytkownika
        public async Task<IActionResult> Unlock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.LockoutEnd = null;
            user.LockoutEnabled = false;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _activityService.LogActivity(User.Identity.Name, "Unlock User", $"Użytkownik {user.UserName} został odblokowany.");
                return RedirectToAction("Index");
            }

            await _activityService.LogActivity(User.Identity.Name, "Unlock User", $"Nieudana próba odblokowania użytkownika {user.UserName}.");
            return View("Index");
        }
        public async Task<IActionResult> Logs()
        {
            var logs = await _activityService.GetAllLogsAsync();  // Pobranie wszystkich logów z serwisu
            return View(logs);  // Przekazanie logów do widoku
        }
        

    }
}
