using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DarkMarket.Areas.Identity.Pages.Account
{
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; private set; } = "";

        public IActionResult OnGet(string? email = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage("./Login");
            }

            Email = email;
            return Page();
        }
    }
}
