using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using static HealthConnect.Pages.Admin.Admin_indexModel;

namespace HealthConnect.Pages.Admin.DoctorReportsManage
{
    public class Report_detailModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;


        public List<User_Table> User_Table = new List<User_Table>();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public List<CountryCount> UserTable { get; set; }
        public List<Doctor_Report> DoctorReport = new List<Doctor_Report>();



        [BindProperty]
        public Doctor_Report Doctor_Report { get; set; }


        public Report_detailModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public Dictionary<string, int> CountryCounts { get; set; }

        public IActionResult OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId.HasValue)
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
                                UserId = reader["id"] as int?;
                            }
                        }
                        con.Close();
                    }
                }

                if (Role != "Admin")
                {
                    return RedirectToPage("/index");
                }
            }
            else
            {
                return RedirectToPage("/index");
            }

            int? DoctorReportId = null;
            if (Request.Query.ContainsKey("doctor_report_id") && int.TryParse(Request.Query["doctor_report_id"], out int parseddoctor_report_id))
            {
                DoctorReportId = parseddoctor_report_id;
            }

            Doctor_Report = GetDoctorReportById(DoctorReportId);



            return Page();

        }

        public Doctor_Report GetDoctorReportById(int? doctor_report_id)
        {
            if (doctor_report_id == null)
                return null;

            Doctor_Report report = null;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT 
            DR.doctor_report_id, 
            DR.user_id, 
            DR.doctor_id, 
            DR.report_message,
            UT.id, UT.first_name, UT.last_name, UT.email, UT.profile_pic,
            UTD.id, UTD.first_name, UTD.last_name, UTD.email, UTD.profile_pic
            FROM Doctor_Report DR
            LEFT JOIN User_Table UT ON DR.user_id = UT.id
            LEFT JOIN User_Table UTD ON DR.doctor_id = UTD.id
            WHERE DR.doctor_report_id = @doctor_report_id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@doctor_report_id", doctor_report_id);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            report = new Doctor_Report
                            {
                                doctor_report_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                doctor_id = reader.GetInt32(2),
                                report_message = reader.IsDBNull(3) ? null : reader.GetString(3),

                                UserList = new User_Table
                                {
                                    id = reader.GetInt32(4),
                                    first_name = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    last_name = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    email = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    profile_pic = reader.IsDBNull(8) ? null : reader.GetString(8)
                                },

                                DoctorList = new User_Table
                                {
                                    id = reader.GetInt32(9),
                                    first_name = reader.IsDBNull(10) ? null : reader.GetString(10),
                                    last_name = reader.IsDBNull(11) ? null : reader.GetString(11),
                                    email = reader.IsDBNull(12) ? null : reader.GetString(12),
                                    profile_pic = reader.IsDBNull(13) ? null : reader.GetString(13)
                                }
                            };
                        }
                    }
                }
            }
            return report;
        }


    }
}
