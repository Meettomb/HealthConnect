using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace HealthConnect.Pages.Admin
{
    public class Admin_indexModel : PageModel
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


        public Admin_indexModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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

            GetTopFiveUser();
            GetUserCountsByCountry(); 
            GetTopFiveDoctorReports();

            return Page();

        }

        private void GetTopFiveUser()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 5 * FROM User_Table ORDER By account_create_date DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User_Table user = new User_Table
                            {
                                id = reader.GetInt32(0),
                                first_name = reader.GetString(1),
                                last_name = reader.GetString(2),
                                email = reader.GetString(3),
                                country = reader.GetString(7),
                                city = reader.GetString(8),
                                gender = reader.GetString(11),
                                role = reader.GetString(12),
                                profile_pic = reader.IsDBNull(14) ? null : reader.GetString(14),
                                account_create_date = reader.GetDateTime(27),
                                block = reader.IsDBNull(28) ? (bool?)null : reader.GetBoolean(28)
                            };
                            User_Table.Add(user);
                        }
                    }
                    con.Close();
                }
            }
        }

        public void GetUserCountsByCountry()
        {
            List<User_Table> userList = new List<User_Table>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table ORDER BY country"; // Optionally, you can add WHERE clause or specific fields if needed

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User_Table user = new User_Table
                            {
                                country = reader.GetString(7) // Index 7 for the country column
                            };
                            userList.Add(user);
                        }
                    }
                }
            }

            // Group by country and count the occurrences
            var countryCounts = userList
                .GroupBy(u => u.country)
                .Select(g => new CountryCount { Country = g.Key, Count = g.Count() })
                .OrderBy(x => x.Country) // Sort by country
                .ToList();

            // Print or assign to a property for use in Razor pages
            foreach (var item in countryCounts)
            {
                Console.WriteLine($"{item.Country} = {item.Count}");
            }

            // Assign to the property of type List<CountryCount>
            UserTable = countryCounts;
        }

        public void GetTopFiveDoctorReports()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT TOP 5 
                         DR.doctor_report_id, 
                         DR.user_id, 
                         DR.doctor_id, 
                         DR.report_message,
                         UT.id,
                         UT.first_name, 
                         UT.last_name, 
                         UT.email, 
                         UT.profile_pic                
                     FROM Doctor_Report DR
                     LEFT JOIN User_Table UT ON DR.user_id = UT.id
                     ORDER BY DR.doctor_report_id DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Doctor_Report report = new Doctor_Report
                            {
                                doctor_report_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                doctor_id = reader.GetInt32(2),
                                report_message = reader.GetString(3),

                                UserList = reader.IsDBNull(4) ? null : new User_Table
                                {
                                    id = reader.GetInt32(4),
                                    first_name = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    last_name = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    email = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    profile_pic = reader.IsDBNull(8) ? null : reader.GetString(8)
                                }
                            };
                            DoctorReport.Add(report);
                        }
                    }
                }
            }
        }


        public class CountryCount
        {
            public string Country { get; set; }
            public int Count { get; set; }
        }



    }
}
