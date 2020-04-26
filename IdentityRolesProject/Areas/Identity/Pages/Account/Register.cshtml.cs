using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IdentityRolesProject.Models.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using IdentityRolesProject.Models.Users;
using IdentityRolesProject.Models.ViewModels.User;
using IdentityRolesProject.Models.Localization;
using IdentityRolesProject.Models.ReCaptcha;
using IdentityRolesProject.Data;
using Microsoft.Extensions.Options;

namespace IdentityRolesProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ITimeZones _timeZones;
        private readonly IReCapchaService _recaptcha;
        private readonly IOptions<SeedDataValues> _seedData;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ITimeZones timeZones, IReCapchaService recaptcha,
            IOptions<SeedDataValues> seedData)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _timeZones = timeZones;
            _recaptcha = recaptcha;
            _seedData = seedData;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public List<SelectListItem> TimeZones { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel : ApplicationUserRegisterVM
        {
            public string ReCaptchaToken { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            Input = new InputModel();


            TimeZones = _timeZones.LoadTimeZones();

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var recaptchaMsj = await _recaptcha.GetResponse(Input.ReCaptchaToken);
            if (!recaptchaMsj.success && recaptchaMsj.score <= 0.5)
            {
                ModelState.AddModelError(string.Empty, "Invalid Bot Register attempt.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email,
                    TimeZone = Input.TimeZone,
                    UserCreationDateUTC = DateTime.UtcNow
                };
                try
                {
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        await _userManager.AddToRoleAsync(user, _seedData.Value.UserRoleName);

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedEmail)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception e)
                {
                    if (e.InnerException.Message.Contains("Cannot insert duplicate key row in object"))
                    {
                        ModelState.AddModelError(string.Empty, "User Name Already Exists");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error Creating The User");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            TimeZones = _timeZones.LoadTimeZones();
            return Page();
        }

    }
}
