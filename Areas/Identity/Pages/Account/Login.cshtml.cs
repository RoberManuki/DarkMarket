using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DarkMarket.Models;
using DarkMarket.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DarkMarket.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AdminSecurityPolicyService _securityPolicyService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AdminSecurityPolicyService securityPolicyService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _securityPolicyService = securityPolicyService;
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

            public bool RememberMe { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var policy = await _securityPolicyService.GetRuntimePolicyAsync();
                var user = await FindByLoginIdentifierAsync(Input.Email);

                if (user is null)
                {
                    ModelState.AddModelError(string.Empty, "Login inválido.");
                    return Page();
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Conta temporariamente bloqueada por tentativas inválidas. Tente novamente em alguns minutos.");
                    return Page();
                }

                if (policy.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Login não permitido. Confirme seu e-mail antes de entrar.");
                    return Page();
                }

                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    await RegisterFailedAttemptAsync(user, policy);
                    return Page();
                }

                await _userManager.ResetAccessFailedCountAsync(user);
                await _signInManager.SignInAsync(user, Input.RememberMe);
                return LocalRedirect("/dashboard");

            }
            return Page();
        }

        private async Task<ApplicationUser?> FindByLoginIdentifierAsync(string email)
        {
            var user = await _userManager.FindByNameAsync(email);
            if (user is not null)
            {
                return user;
            }

            return await _userManager.FindByEmailAsync(email);
        }

        private async Task RegisterFailedAttemptAsync(ApplicationUser user, RuntimeSecurityPolicy policy)
        {
            if (!user.LockoutEnabled)
            {
                ModelState.AddModelError(string.Empty, "Login inválido.");
                return;
            }

            await _userManager.AccessFailedAsync(user);
            var failedCount = await _userManager.GetAccessFailedCountAsync(user);

            if (failedCount >= policy.LockoutMaxFailedAccessAttempts)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(policy.LockoutMinutes));
                await _userManager.ResetAccessFailedCountAsync(user);

                ModelState.AddModelError(string.Empty, "Conta temporariamente bloqueada por tentativas inválidas. Tente novamente em alguns minutos.");
                return;
            }

            ModelState.AddModelError(string.Empty, "Login inválido.");
        }
    }
}