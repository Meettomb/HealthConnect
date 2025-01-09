using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.UserData
{
    public class Register_inquiryModel : PageModel
    {
        public List<Doctor_approvel> Doctor_approvel = new List<Doctor_approvel>();

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Register_inquiryModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            {
                string query = "SELECT * FROM Doctor_approvel WHERE account_approve IS NULL";
                using SqlCommand command = new SqlCommand(query, connection);
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            Doctor_approvel doctor = new Doctor_approvel();
                            doctor.doctor_approvel_id = reader.GetInt32(0);
                            doctor.first_name = reader.GetString(1);
                            doctor.last_name = reader.GetString(2);
                            doctor.email = reader.GetString(3);
                            doctor.profile_pic = reader.GetString(14);
                            doctor.medical_registration_no = reader.GetString(16);
                            doctor.state_medical_council = reader.GetString(17);
                            doctor.year_of_registration = reader.GetString(18);
                            doctor.doctor_qualifications = reader.GetString(21);
                            doctor.account_create_date = DateTime.Parse(reader.GetString(27));
                            Doctor_approvel.Add(doctor);
                        }
                        connection.Close();
                    }
                }
            }
            return Page();
        }
    }
}
