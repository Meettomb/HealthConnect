using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using HealthConnect.Models;
using Microsoft.Data.SqlClient;
using static HealthConnect.Pages.IndexModel;
using HealthConnect.Services;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Account.Settings
{
    public class Change_emailModel : PageModel
    {

        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public Change_emailModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, ILogger<IndexModel> logger, IConfiguration configuration)
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
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Dob { get; set; }
        public bool? DoctorProfileComplete { get; set; }
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
                            Country = reader["country"].ToString();
                            State = reader["state"].ToString();
                            City = reader["city"].ToString();
                            UserId = (int?)reader["id"];
                            DoctorProfileComplete = reader["doctor_profile_complete"] != DBNull.Value ? (bool?)(Convert.ToInt32(reader["doctor_profile_complete"]) == 1) : null;
                            Dob = reader["dob"].ToString();
                            Dob = reader["House_number_and_Street_name"].ToString();
                            Dob = reader["pincode"].ToString();


                        }
                    }
                    con.Close();
                }
            }
        }

      
        public async Task<IActionResult> OnPostAsync()
        {
            HttpContext.Session.SetInt32("id", User.id);
            HttpContext.Session.SetString("email", User.email);


            var otp = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OtpGeneratedTime", DateTime.Now.ToString("o"));

            string subject = "Your OTP for HealthConnect Registration";
            string body = $"Your OTP is: {otp}";
            await _emailService.SendEmailAsync(User.email, subject, body);

            return RedirectToPage("/OTPVerify/Change_email_otp_verify");
        }

    }
}
