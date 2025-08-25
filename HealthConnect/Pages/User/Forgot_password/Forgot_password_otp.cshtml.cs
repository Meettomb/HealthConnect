using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace HealthConnect.Pages.User.Forgot_password
{
    public class Forgot_password_otpModel : PageModel
    {
        private readonly IEmailService _emailService;

        public Forgot_password_otpModel(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public string OtpInput { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            var storedOtp = HttpContext.Session.GetString("Otp");
            var email = HttpContext.Session.GetString("ResetEmail");

            if (storedOtp != OtpInput)
            {
                ErrorMessage = "Invalid OTP.";
                return Page();
            }

            // OTP verified successfully, redirect to New password page
            return RedirectToPage("/User/Forgot_password/New_password_page");
        }
    }
}
