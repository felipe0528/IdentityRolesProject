using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityRolesProject.Models.Email;
using IdentityRolesProject.Models.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IdentityRolesProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class UnconfirmedEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IReCapchaService _recaptcha;

        public UnconfirmedEmailModel(UserManager<IdentityUser> userManager
            , IEmailSender emailSender, IReCapchaService recaptcha)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _recaptcha = recaptcha;
        }

        [TempData]
        public Guid UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            public string ReCaptchaToken { get; set; }
        }

        public async Task OnGetAsync(Guid userId)
        {
            UserId = userId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            Input.Email = user.Email;
            ModelState.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var recaptchaMsj = await _recaptcha.GetResponse(Input.ReCaptchaToken);

            if (!recaptchaMsj.success && recaptchaMsj.score <= 0.5)
            {
                ModelState.AddModelError(string.Empty, "Invalid bot login attempt.");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToPage("./CheckEmail");
                }

                var result = await _userManager.UpdateSecurityStampAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return Page();
                    }
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href = '{HtmlEncoder.Default.Encode(callbackUrl)}' > clicking here </a>. Remember that your User Name is <b>{user.UserName}</b>");

                return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
            }

            return Page();
        }
    }
}
