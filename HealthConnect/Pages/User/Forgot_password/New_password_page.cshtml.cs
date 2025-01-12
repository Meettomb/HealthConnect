using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HealthConnect.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using HealthConnect.Models;

namespace HealthConnect.Pages.User.Forgot_password
{
    public class New_password_pageModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public New_password_pageModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString("ResetEmail");

            Password = Password?.Trim();
            ConfirmPassword = ConfirmPassword?.Trim();

            // Check if passwords match
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            var user = await GetUserByEmailAsync(email);
            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            var passwordHasher = new PasswordHasher<User_Table>();
            var hashedPassword = passwordHasher.HashPassword(null, Password); 

            bool updateSuccess = await UpdateUserPasswordInDatabase(email, hashedPassword);

            if (!updateSuccess)
            {
                ErrorMessage = "Failed to update the password.";
                return Page();
            }

            var subject = "Your Password Has Been Reset";
            var body = $"Hello, your password has been successfully reset. You can now log in with your new password.";
            await _emailService.SendEmailAsync(email, subject, body);

            SuccessMessage = "Your password has been successfully reset. A confirmation email has been sent.";
            TempData["SuccessMessage"] = SuccessMessage;

            return RedirectToPage("/User/Sign_in");
        }

        private async Task<User_Table> GetUserByEmailAsync(string email)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT * FROM User_Table WHERE Email = @Email";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User_Table
                                {
                                    email = reader["Email"].ToString(),
                                    password = reader["Password"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return null;
        }

        private async Task<bool> UpdateUserPasswordInDatabase(string email, string hashedPassword)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "UPDATE User_Table SET password = @Password WHERE Email = @Email";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Password", hashedPassword);
                        command.Parameters.AddWithValue("@Email", email);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
