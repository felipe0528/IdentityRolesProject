using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IdentityRolesProject.Models.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using IdentityRolesProject.Models.ViewModels.User;
using Microsoft.AspNetCore.Mvc.Rendering;
using IdentityRolesProject.Models.Localization;
using IdentityRolesProject.Models.Users;
using IdentityRolesProject.Models.ReCaptcha;
using Microsoft.Extensions.Options;
using IdentityRolesProject.Data;

namespace IdentityRolesProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly ITimeZones _timeZones;
        private readonly IReCapchaService _recaptcha;
        private readonly IOptions<SeedDataValues> _seedData;

        private static string CLAIM_SURNAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        private static string CLAIM_GIVENNAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger,
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

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel : ApplicationUserRegisterVM
        {
            public string ReCaptchaToken { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            if (result.IsNotAllowed)
            {
                return RedirectToPage("RegisterConfirmation", new { email = info.Principal.FindFirstValue(ClaimTypes.Email) });
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                TimeZones = _timeZones.LoadTimeZones();

                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;
                Input = new InputModel();
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                }
                if (info.Principal.HasClaim(c => c.Type == CLAIM_SURNAME))
                {
                    Input.LastName = info.Principal.FindFirstValue(CLAIM_SURNAME);
                }
                if (info.Principal.HasClaim(c => c.Type == CLAIM_GIVENNAME))
                {
                    Input.FirstName = info.Principal.FindFirstValue(CLAIM_GIVENNAME);
                }
                Input.Password = "N0Password!";
                Input.ConfirmPassword = "N0Password!";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var recaptchaMsj = await _recaptcha.GetResponse(Input.ReCaptchaToken);

            if (!recaptchaMsj.success && recaptchaMsj.score <= 0.5)
            {
                ModelState.AddModelError(string.Empty, "Invalid Bot Register attempt.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = Input.UserName,
                        Email = Input.Email,
                        TimeZone = Input.TimeZone,
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        UserCreationDateUTC = DateTime.UtcNow
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                            await _userManager.AddToRoleAsync(user, _seedData.Value.UserRoleName);

                            // If account confirmation is required, we need to show the link if we don't have a real email sender
                            if (_userManager.Options.SignIn.RequireConfirmedAccount)
                            {
                                return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                            }

                            await _signInManager.SignInAsync(user, isPersistent: false);
                            var userId = await _userManager.GetUserIdAsync(user);
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = userId, code = code },
                                protocol: Request.Scheme);

                            await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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
            TimeZones = _timeZones.LoadTimeZones();
            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
