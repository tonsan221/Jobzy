﻿namespace Jobzy.Web.Areas.Identity.Pages.Account
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;

    using Jobzy.Common;
    using Jobzy.Data.Models;
    using Jobzy.Services.Interfaces;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;

    using Stripe;

    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFreelancePlatform freelancePlatform;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            IFreelancePlatform freelancePlatform,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            this.freelancePlatform = freelancePlatform;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._logger = logger;
            this._emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Range(0, 1, ErrorMessage = "An invalid role has been chosen.")]
            public UserType UserType { get; set; }

            [Required]
            [StringLength(24, MinimumLength = 3, ErrorMessage = "The {0} must be between {2} and {1} characters long.")]
            [Display(Name = "Full Name")]
            public string Name { get; set; }

            [Required]
            [StringLength(24, MinimumLength = 3, ErrorMessage = "The {0} must be between {2} and {1} characters long.")]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Location")]
            public Country Location { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The Password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Connect to Stripe")]
            public bool HasStripeAccount { get; set; }
        }

        public async Task OnGetAsync(string code)
        {
            this.Input = new InputModel { HasStripeAccount = false };

            if (code != null)
            {
                this.Input.HasStripeAccount = true;
            }

            this.ExternalLogins = (await this._signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= this.Url.Content("~/");
            this.ExternalLogins = (await this._signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (this.ModelState.IsValid)
            {
                ApplicationUser user;

                switch (this.Input.UserType)
                {
                    case UserType.Freelancer:
                        user = new Freelancer();
                        break;
                    case UserType.Employer:
                        user = new Employer();
                        break;
                    default:
                        this.ModelState.AddModelError(null, "Choose your role.");
                        return this.Page();
                }

                var account =
                    this.freelancePlatform.StripeAccountManager.CreateAccount(
                        this.Input.Name, this.Input.Email, this.Input.Location);

                user.Id = account.Id;
                user.Name = this.Input.Name;
                user.UserName = this.Input.Username;
                user.Email = this.Input.Email;
                user.Location = this.Input.Location;
                user.Balance = new Data.Models.Balance();

                var result = await this._userManager.CreateAsync(user, this.Input.Password);

                await this._userManager.AddToRoleAsync(user, this.Input.UserType.ToString());

                if (result.Succeeded)
                {
                    this._logger.LogInformation("User created a new account with password.");

                    var code = await this._userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = this.Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: this.Request.Scheme);

                    await this._emailSender.SendEmailAsync(this.Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (this._userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return this.RedirectToPage("RegisterConfirmation", new { email = this.Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await this._signInManager.SignInAsync(user, isPersistent: false);
                        return this.LocalRedirect("/");
                    }
                }

                foreach (var error in result.Errors)
                {
                    this.ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return this.Page();
        }
    }
}
