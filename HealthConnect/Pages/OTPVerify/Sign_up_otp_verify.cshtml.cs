using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.OTPVerify
{
    public class Sign_up_otp_verifyModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Sign_up_otp_verifyModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public string EnteredOTP { get; set; }

        public string sessionOtp => HttpContext.Session.GetString("OTP");

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Gender { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string Account_create_date { get; set; }
        public string CurrencyCode { get; set; }

        public string SessionOtp { get; set; }

        public IActionResult OnGet()
        {
            // Retrieve data from session
            FirstName = HttpContext.Session.GetString("FirstName");
            LastName = HttpContext.Session.GetString("LastName");
            Email = HttpContext.Session.GetString("Email");
            MobileNo = HttpContext.Session.GetString("MobileNo");
            DOB = HttpContext.Session.GetString("DOB");
            Address = HttpContext.Session.GetString("Address");
            Country = HttpContext.Session.GetString("Country");
            City = HttpContext.Session.GetString("City");
            State = HttpContext.Session.GetString("State");
            Pincode = HttpContext.Session.GetString("Pincode");
            Gender = HttpContext.Session.GetString("Gender");
            Role = HttpContext.Session.GetString("Role");
            Password = HttpContext.Session.GetString("Password");
            Account_create_date = HttpContext.Session.GetString("Account_create_date");
            CurrencyCode = HttpContext.Session.GetString("CurrencyCode");

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
                ErrorMessage = "OTP timestamp not found. Please restart the registration process.";
                return Page();
            }

            if (DateTime.TryParse(otpGeneratedTimeString, out var otpGeneratedTime))
            {
                if (DateTime.Now > otpGeneratedTime.AddMinutes(2))
                {
                    ErrorMessage = "OTP has expired. Please try registering again.";
                    return Page();
                }
            }
            else
            {
                ErrorMessage = "Invalid OTP timestamp. Please restart the registration process.";
                return Page();
            }

            if (EnteredOTP.Trim() == sessionOtp.Trim())
            {
                SuccessMessage = "Registration Completed";
                StoreUserInDatabase();
                HttpContext.Session.Clear();

                string subject = "Your Registration";
                string body = "Your registration is successfully completed on HealthConnect.";
                string grantedMessage = "Thank you for registering with HealthConnect!";
                string companyName = "HealthConnect";
                string combinedBody = $"{body}\n\n{grantedMessage}\n\nRegards,\n{companyName}";

                await _emailService.SendEmailAsync(HttpContext.Session.GetString("Email"), subject, combinedBody);
            

            return RedirectToPage("/User/Sign_in");
            }

            else
            {
                ErrorMessage = "Invalid OTP. Please try again.";
                return Page();
            }
        }

        private async void StoreUserInDatabase()
        {
            var user = new User_Table
            {
                first_name = HttpContext.Session.GetString("FirstName"),
                last_name = HttpContext.Session.GetString("LastName"),
                email = HttpContext.Session.GetString("Email"),
                mobil_no = HttpContext.Session.GetString("MobileNo"),
                dob = HttpContext.Session.GetString("DOB"),
                House_number_and_Street_name = HttpContext.Session.GetString("Address"),
                country = HttpContext.Session.GetString("Country"),
                city = HttpContext.Session.GetString("City"),
                state = HttpContext.Session.GetString("State"),
                pincode = HttpContext.Session.GetString("Pincode"),
                gender = HttpContext.Session.GetString("Gender"),
                role = HttpContext.Session.GetString("Role"),
                currency_code = HttpContext.Session.GetString("CurrencyCode"),
                password = new PasswordHasher<User_Table>().HashPassword(null, HttpContext.Session.GetString("Password")),
                account_approve = true,
                isactive = true,
                block = false,
                mobail_verifie = false
            };

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO User_Table (first_name, last_name, email, mobil_no, dob, House_number_and_Street_name, country, city, state, pincode, gender, role, password, isactive, account_approve, Account_create_date, block, mobail_verifie, currency_code) " +
                               "VALUES (@first_name, @last_name, @email, @mobil_no, @dob, @House_number_and_Street_name, @country, @city, @state, @pincode, @gender, @role, @password, @isactive, @account_approve, @Account_create_date, @Block, @Mobail_verifie, @Currency_code)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@first_name", user.first_name);
                    command.Parameters.AddWithValue("@last_name", user.last_name);
                    command.Parameters.AddWithValue("@email", user.email);
                    command.Parameters.AddWithValue("@mobil_no", user.mobil_no);
                    command.Parameters.AddWithValue("@dob", user.dob);
                    command.Parameters.AddWithValue("@House_number_and_Street_name", user.House_number_and_Street_name);
                    command.Parameters.AddWithValue("@country", user.country);
                    command.Parameters.AddWithValue("@city", user.city);
                    command.Parameters.AddWithValue("@state", user.state);
                    command.Parameters.AddWithValue("@pincode", user.pincode);
                    command.Parameters.AddWithValue("@gender", user.gender);
                    command.Parameters.AddWithValue("@role", user.role);
                    command.Parameters.AddWithValue("@password", user.password);
                    command.Parameters.AddWithValue("@account_approve", user.account_approve);
                    command.Parameters.AddWithValue("@isactive", user.isactive);
                    command.Parameters.AddWithValue("@Block", user.block);
                    command.Parameters.AddWithValue("@Mobail_verifie", user.mobail_verifie);
                    command.Parameters.AddWithValue("@Currency_code", user.currency_code);

                    string accountCreateDateString = HttpContext.Session.GetString("Account_create_date");
                    if (DateTime.TryParse(accountCreateDateString, out DateTime accountCreateDate))
                    {
                        command.Parameters.AddWithValue("@Account_create_date", accountCreateDate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Account_create_date", DateTime.Now);
                    }

                    command.ExecuteNonQuery();
                }
            }

            // Send a confirmation email after saving to the database
            string subject = "Registration Completed";
            string body = "Your registration is successfully completed on HealthConnect.";
            string combinedBody = $"{body}\n\nThank you for registering with HealthConnect!\n\n";

            await _emailService.SendEmailAsync(user.email, subject, combinedBody);
        }

    }
}
