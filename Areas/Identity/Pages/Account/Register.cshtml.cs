using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using DarkMarket.Models;

namespace DarkMarket.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _environment;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _environment = environment;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    // Aqui adiciona a role "user"
                    await _userManager.AddToRoleAsync(user, "user");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code },
                        protocol: Request.Scheme);

                    if (callbackUrl != null)
                    {
                        await _emailSender.SendEmailAsync(
                            Input.Email,
                            "Confirme seu e-mail",
                            $"Confirme sua conta clicando aqui: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>confirmar conta</a>.");
                    }

                    if (_environment.IsDevelopment())
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect("~/dashboard");
                    }

                    return RedirectToPage("./RegisterConfirmation", new { email = Input.Email });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }
    }
}