using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthConnect.Pages.OTPVerify
{
    public class Change_password_otp_verifyModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Change_password_otp_verifyModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public string EnteredOTP { get; set; }
        public int? UserId { get; set; }
        public string SessionEmail { get; set; }
        public string SessionPassword { get; set; }

        public string sessionOtp => HttpContext.Session.GetString("OTP");


        public string SessionOtp { get; set; }

        public IActionResult OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");

            SessionEmail = HttpContext.Session.GetString("email");
            SessionOtp = HttpContext.Session.GetString("OTP");
            SessionPassword = HttpContext.Session.GetString("Password");

            if (!UserId.HasValue || string.IsNullOrEmpty(SessionEmail) ||
                string.IsNullOrEmpty(SessionOtp) || string.IsNullOrEmpty(SessionPassword))
            {
                TempData["ErrorMessage"] = "Session expired. Please try again.";
                return RedirectToPage("/Account/Settings/Change_password");
            }

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
                SuccessMessage = "Password Updated";
                await UpdatePassword();
                HttpContext.Session.Clear();

                TempData["SuccessMessage"] = "Your password has been successfully updated.";

                string subject = "Your Password Update";
                string body = "Your Password is successfully updated on HealthConnect.";
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


        public async Task UpdatePassword()
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            string newPassword = HttpContext.Session.GetString("Password");

            if (!userId.HasValue || string.IsNullOrEmpty(newPassword))
            {
                ErrorMessage = "Invalid session data. UserId or Password is missing.";
                return;
            }

            var user = new User_Table();
            var passwordHasher = new PasswordHasher<User_Table>();
            string hashedPassword = passwordHasher.HashPassword(user, newPassword);

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE User_Table SET password = @password WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", userId.Value);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ErrorMessage = "No user found with this ID. Password update failed.";
                            return;
                        }
                    }
                }

                string subject = "Your Password Update";
                string body = "Your password has been successfully updated on HealthConnect.";

                await _emailService.SendEmailAsync(HttpContext.Session.GetString("email"), subject, body);
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating the password: " + ex.Message;
            }
        }




    }
}
