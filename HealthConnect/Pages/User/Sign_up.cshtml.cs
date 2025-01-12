using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Http;

namespace HealthConnect.Pages.User
{
    public class Sign_upModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Sign_upModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        [BindProperty]
        public User_Table user { get; set; }

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill out all required fields.";
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string checkEmailQuery = "SELECT COUNT(*) FROM User_Table WHERE email = @Email";
                string checkPhoneQuery = "SELECT COUNT(*) FROM User_Table WHERE mobil_no = @MobileNo";

                using (SqlCommand checkEmailCommand = new SqlCommand(checkEmailQuery, connection))
                {
                    checkEmailCommand.Parameters.AddWithValue("@Email", user.email);
                    int emailCount = (int)checkEmailCommand.ExecuteScalar();

                    if (emailCount > 0)
                    {
                        ErrorMessage = "This email is already in use.";
                        return Page();
                    }
                }

                using (SqlCommand checkPhoneCommand = new SqlCommand(checkPhoneQuery, connection))
                {
                    checkPhoneCommand.Parameters.AddWithValue("@MobileNo", user.mobil_no);
                    int phoneCount = (int)checkPhoneCommand.ExecuteScalar();

                    if (phoneCount > 0)
                    {
                        ErrorMessage = "This mobile number is already in use.";
                        return Page();
                    }
                }
            }

            // Set session values for user data
            HttpContext.Session.SetString("FirstName", user.first_name);
            HttpContext.Session.SetString("LastName", user.last_name);
            HttpContext.Session.SetString("Email", user.email);
            HttpContext.Session.SetString("MobileNo", user.mobil_no);
            HttpContext.Session.SetString("DOB", user.dob);
            HttpContext.Session.SetString("Address", user.House_number_and_Street_name);
            HttpContext.Session.SetString("Country", user.country);
            HttpContext.Session.SetString("City", user.city);
            HttpContext.Session.SetString("State", user.state);
            HttpContext.Session.SetString("Pincode", user.pincode);
            HttpContext.Session.SetString("Gender", user.gender);
            HttpContext.Session.SetString("Role", user.role);
            HttpContext.Session.SetString("Password", user.password); 
            HttpContext.Session.SetString("Account_create_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));


            if (user.role == "User")
            {
                var otp = new Random().Next(100000, 999999).ToString();
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OtpGeneratedTime", DateTime.Now.ToString("o"));

                string subject = "Your OTP for HealthConnect Registration";
                string body = $"Your OTP is: {otp}";
                await _emailService.SendEmailAsync(user.email, subject, body);

                return RedirectToPage("/OTPVerify/Sign_up_otp_verify");
            }

            else if (user.role == "Doctor")
            {
                var otp = new Random().Next(100000, 999999).ToString();
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OtpGeneratedTime", DateTime.Now.ToString("o"));

                string subject = "Your OTP for HealthConnect Registration";
                string body = $"Your OTP is: {otp}";
                await _emailService.SendEmailAsync(user.email, subject, body);

                return RedirectToPage("/OTPVerify/Doctor_opt_verify");
            }

            return Page();
        }


    }
}
