using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using static HealthConnect.Pages.IndexModel;

namespace HealthConnect.Pages.Account.Settings
{
    public class Change_passwordModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public Change_passwordModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }



        [BindProperty]
        public int id { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }


        [BindProperty]
        public User_Table User { get; set; }
        public List<User_Table> User_list = new List<User_Table>();

        public List<User_Table> User_TableIdList { get; set; } = new List<User_Table>();
        public List<DoctorCount> DoctorCounts { get; set; } = new List<DoctorCount>();


        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

        public int TotalSpecialitiesCount { get; set; }
        public int TotalUsersCount { get; set; }
        public int TotalDoctorsCount { get; set; }

        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId.HasValue)
            {
                OnGetLoginUserDetail();
            }
            else
            {
                return RedirectToPage("/User/Sign_in");
            }

            return Page();
        }

        public void OnGetLoginUserDetail()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId.Value);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FirstName = reader["first_name"].ToString();
                            LastName = reader["last_name"].ToString();
                            ProfilePic = reader["profile_pic"].ToString();
                            Role = reader["role"].ToString();
                            Password = reader["password"].ToString();

                            Email = reader["email"].ToString();
                            UserId = (int?)reader["id"];
                        }
                    }
                    con.Close();
                }
            }
        }


        public async Task<IActionResult> OnPostAsync()
        {
            string enteredCurrentPassword = Request.Form["current_password"];
            string newPassword = Request.Form["password"];
            string id = Request.Form["id"];

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT password, email FROM User_Table WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedHashedPassword = reader["password"].ToString();
                            string email = reader["email"].ToString();

                            var user = new User_Table();
                            var passwordHasher = new PasswordHasher<User_Table>();
                            var verificationResult = passwordHasher.VerifyHashedPassword(user, storedHashedPassword, enteredCurrentPassword);

                            if (verificationResult == PasswordVerificationResult.Success)
                            {
                                var otp = new Random().Next(100000, 999999).ToString();
                                HttpContext.Session.SetString("OTP", otp);
                                HttpContext.Session.SetString("OtpGeneratedTime", DateTime.Now.ToString("o"));
                                HttpContext.Session.SetString("Password", newPassword);
                                HttpContext.Session.SetString("Id", id);
                                HttpContext.Session.SetString("email", email);

                                string subject = "Your OTP for HealthConnect";
                                string body = $"Your OTP is: {otp}";
                                await _emailService.SendEmailAsync(email, subject, body);

                                return RedirectToPage("/OTPVerify/Change_password_otp_verify");
                            }
                        }
                    }
                    con.Close();
                }
            }

            ErrorMessage = "Incorrect current password. Please try again.";
            TempData["ErrorMessage"] = "Incorrect current password. Please try again.";
            return RedirectToPage();
        }
    
    }
}
