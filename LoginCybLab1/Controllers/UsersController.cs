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

        public UsersController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
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
                // Zapisz nowe ustawienia w pliku konfiguracyjnym lub bazie danych
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

    // READ: Wyświetlenie listy użytkowników
    public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // CREATE: GET - Wyświetlenie formularza dodawania nowego użytkownika
        [HttpGet]
        public IActionResult Create()
        {
            return View();
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
                    return RedirectToAction("Index");

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

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
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

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
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(user);
        }
        //blokowanie
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

            // Blokowanie użytkownika na określoną liczbę dni
            user.LockoutEnd = DateTimeOffset.UtcNow.AddDays(days);
            user.LockoutEnabled = true;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Index");
        }


        //odblokowanie
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

            // Odblokowanie użytkownika
            user.LockoutEnd = null; // Ustawienie na null oznacza odblokowanie
            user.LockoutEnabled = false;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Index");
        }



    }
}
