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

        public int? UserId { get; set; }

        public IActionResult OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            string roleInSession = HttpContext.Session.GetString("UserRole");

            if (UserId.HasValue && !string.IsNullOrEmpty(roleInSession))
            {
                if (roleInSession == "Admin")
                {
                    return RedirectToPage("/Admin/Admin_index");
                }
                else if (roleInSession == "User" || roleInSession == "Doctor")
                {
                    return RedirectToPage("/index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid role in session.");
                }
            }

            string deviceId = Request.Cookies["deviceUniqueId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT id, first_name, last_name, email, role, isactive 
                         FROM User_Table 
                         WHERE ',' + auth_token + ',' LIKE '%,' + @DeviceId + ',%'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["id"]);
                        string firstName = reader["first_name"].ToString();
                        string lastName = reader["last_name"].ToString();
                        string email = reader["email"].ToString();
                        string role = reader["role"].ToString();
                        bool isActive = Convert.ToBoolean(reader["isactive"]);

                        if (isActive)
                        {
                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("FirstName", firstName);
                            HttpContext.Session.SetString("LastName", lastName);
                            HttpContext.Session.SetString("Email", email);
                            HttpContext.Session.SetString("UserRole", role);

                            // Redirect based on role
                            if (role == "Admin")
                            {
                                return RedirectToPage("/Admin/Admin_index");
                            }
                            else if (role == "User" || role == "Doctor")
                            {
                                return RedirectToPage("/index");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid role.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Account is not active.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Device not recognized or account not found.");
                    }
                }
            }

            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
        {

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
            HttpContext.Session.SetString("CurrencyCode", user.currency_code); 


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

            else if(user.role == "Pharmacist")
            {
                var otp = new Random().Next(100000, 999999).ToString();
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OtpGeneratedTime", DateTime.Now.ToString("o"));

                string subject = "Your OTP for HealthConnect Registration";
                string body = $"Your OTP is: {otp}";
                await _emailService.SendEmailAsync(user.email, subject, body);

                return RedirectToPage("/OTPVerify/Pharmacist_opt_verify");
            }
            return Page();
        }


    }
}
