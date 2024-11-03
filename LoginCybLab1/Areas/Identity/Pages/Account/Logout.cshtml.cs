// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using LoginCybLab1.Models;


namespace LoginCybLab1.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IUserActivityService _activityService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger, IUserActivityService activityService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            var userEmail = User.Identity.Name;

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            await _activityService.LogLogout(userEmail);
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
