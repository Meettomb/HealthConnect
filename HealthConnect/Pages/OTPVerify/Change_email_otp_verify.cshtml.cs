using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.OTPVerify
{
    public class Change_email_otp_verifyModel : PageModel
    {

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Change_email_otp_verifyModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public string EnteredOTP { get; set; }
        public string SessionId { get; set; }
        public string SessionEmail { get; set; }

        public string sessionOtp => HttpContext.Session.GetString("OTP");


        public string SessionOtp { get; set; }

        public IActionResult OnGet()
        {
            SessionId = HttpContext.Session.GetString("id");
            SessionEmail= HttpContext.Session.GetString("email");
            SessionOtp = HttpContext.Session.GetString("OTP");

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var sessionOtp = HttpContext.Session.GetString("OTP");
            var otpGeneratedTimeString = HttpContext.Session.GetString("OtpGeneratedTime");

            if (string.IsNullOrEmpty(sessionOtp))
            {
                ErrorMessage = "OTP not found. Please restart the registration process.";
                return Page();
            }

            if (string.IsNullOrEmpty(otpGeneratedTimeString))
            {
                ErrorMessage = "OTP timestamp not found. Please restart the process.";
                return Page();
            }

            if (EnteredOTP.Trim() == sessionOtp.Trim())
            {
                SuccessMessage = "Email Updated";
                await UpdateEmail();
                HttpContext.Session.Clear();

                TempData["SuccessMessage"] = "Your email has been successfully updated.";

                string subject = "Your Email Update";
                string body = "Your Email is successfully updated on HealthConnect.";
                string grantedMessage = "Thank you for registering with HealthConnect!";
                string companyName = "HealthConnect";
                string combinedBody = $"{body}\n\n{grantedMessage}\n\nRegards,\n{companyName}";

                await _emailService.SendEmailAsync(HttpContext.Session.GetString("email"), subject, combinedBody);

                return RedirectToPage("/Account/Overview");
            }

            TempData["ErrorMessage"] = "Invalid OTP. Please try again.";
            ErrorMessage = "Invalid OTP. Please try again.";
            return Page();
        }


        public async Task UpdateEmail()
        {
            var userId = HttpContext.Session.GetInt32("id");
            var newEmail = HttpContext.Session.GetString("email");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE User_Table SET email = @email WHERE id = @id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", userId);
                    command.Parameters.AddWithValue("@email", newEmail);

                    await command.ExecuteNonQueryAsync();
                }
            }

            string subject = "Your Email Update";
            string body = "Your Email is successfully updated on HealthConnect.";

            await _emailService.SendEmailAsync(newEmail, subject, body);

        }


    }
}
