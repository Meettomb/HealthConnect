using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HealthConnect.Pages.User.Forgot_password
{
    public class Enter_EmailModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Enter_EmailModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public User_Table user { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(user.email))
            {
                ErrorMessage = "Please enter your email.";
                return Page();
            }

            var query = "SELECT email FROM User_Table WHERE email = @Email";
            string foundEmail = string.Empty;

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", user.email);
                    await connection.OpenAsync();
                    foundEmail = command.ExecuteScalar() as string;
                }
            }

            if (string.IsNullOrEmpty(foundEmail))
            {
                ErrorMessage = "No account found with that email.";
                return Page();
            }

            var otp = GenerateOtp();
            var emailSent = await _emailService.SendEmailAsync(
                foundEmail,
                "OTP for Password Reset",
                $"Your OTP for password reset is: {otp}");

            if (!emailSent)
            {
                ErrorMessage = "Failed to send OTP. Please try again later.";
                return Page();
            }

            // Store email and OTP in session
            HttpContext.Session.SetString("ResetEmail", foundEmail);
            HttpContext.Session.SetString("Otp", otp);

            // Redirect to OTP verification page
            return RedirectToPage("/User/Forgot_password/Forgot_password_otp");
        }

        private string GenerateOtp()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            return otp;
        }
    }
}
