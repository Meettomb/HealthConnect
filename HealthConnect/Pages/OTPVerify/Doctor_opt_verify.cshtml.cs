using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.OTPVerify
{
    public class Doctor_opt_verifyModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Doctor_opt_verifyModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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

            Console.WriteLine($"Session OTP: {sessionOtp}");
            Console.WriteLine($"Entered OTP: {EnteredOTP}");
            Console.WriteLine($"OTP Generated Time: {otpGeneratedTimeString}");

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
                SuccessMessage = "Email verify Completed";

                return RedirectToPage("/User/Doctor_sign_up");
            }

            else
            {
                ErrorMessage = "Invalid OTP. Please try again.";
                return Page();
            }
        }



    }
}
