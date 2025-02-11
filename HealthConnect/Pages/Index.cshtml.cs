using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using static HealthConnect.Pages.Admin.Admin_indexModel;

namespace HealthConnect.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;


        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public bool? DoctorProfileComplete { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }


        [BindProperty]
        public User_Table User { get; set; }

        public List<User_Table> User_TableList { get; set; } = new List<User_Table>();
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
        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");
            OnIndexSectionDeyailGet();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT doctor_specialitis_id, doctor_specialitis FROM Doctor_Specialitis";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doctorSpecialitiesList.Add(new Doctor_Specialitis
                            {
                                doctor_specialitis_id = reader.GetInt32(0),
                                doctor_specialitis = reader.GetString(1),
                            });
                        }
                    }
                }
            }

            TotalSpecialitiesCount = doctorSpecialitiesList?.Count ?? 0;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT doctor_type_id, type_of_doctor FROM Types_of_Doctor";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Types_of_doctor.Add(new Types_of_Doctor
                            {
                                doctor_type_id = reader.IsDBNull(0) ? 0 : int.Parse(reader.GetString(0)),
                                type_of_doctor = reader.GetString(1),
                            });

                        }
                    }
                }
            }

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
                                Country = reader["country"].ToString();
                                State = reader["state"].ToString();
                                City = reader["city"].ToString();
                                UserId = (int?)reader["id"];
                                DoctorProfileComplete = reader["doctor_profile_complete"] != DBNull.Value ? (bool?)(Convert.ToInt32(reader["doctor_profile_complete"]) == 1) : null;


                            }
                        }
                        con.Close();
                    }
                }
            }

            if (UserId.HasValue && !string.IsNullOrEmpty(roleInSession))
            {
                if (roleInSession == "User" || roleInSession == "Doctor" || roleInSession == "Admin")
                {
                    return Page();
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
                        string role = reader["role"].ToString();
                        bool isActive = Convert.ToBoolean(reader["isactive"]);

                        if (isActive)
                        {
                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("UserRole", role);

                            if (role == "User" || role == "Doctor" || role == "Admin")
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



        public void OnIndexSectionDeyailGet()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string getDoctorCount = "SELECT COUNT(*) FROM User_Table WHERE role = 'Doctor'";
                using (SqlCommand command = new SqlCommand(getDoctorCount, connection))
                {
                    TotalDoctorsCount = (int)command.ExecuteScalar();
                }

                string getTotalUsers = "SELECT COUNT(*) FROM User_Table ";
                using (SqlCommand command = new SqlCommand(getTotalUsers, connection))
                {
                    TotalUsersCount = (int)command.ExecuteScalar(); 
                }
            }

         
        }

        public class DoctorCount
        {
            public string role { get; set; }
            public int Count { get; set; }
        }


        public IActionResult OnPost()
        {
            int? UserId = HttpContext.Session.GetInt32("Id");

            if (!UserId.HasValue)
            {
                ErrorMessage = "User session expired. Please log in again.";
                return Page();
            }

            var selectedDays = Request.Form["weekly_work_days"].ToList();

            if (selectedDays.Count == 0)
            {
                ErrorMessage = "Please select days which you would like to work.";
                return Page();
            }

            User.weekly_work_days = selectedDays;
            OnPostAddDoctorworkingInfo(UserId.Value);

            return RedirectToPage();
        }

        public void OnPostAddDoctorworkingInfo(int UserId)
        {
            if (User.work_start_time == null)
            {
                ErrorMessage = "Please select work start time.";
                return;
            }
            if (User.work_end_time == null)
            {
                ErrorMessage = "Please select work end time.";
                return;
            }
            if (User.weekly_work_days == null || User.weekly_work_days.Count == 0)
            {
                ErrorMessage = "Please select days which you would like to work.";
                return;
            }
            if (User.max_time_per_appointments == null || !int.TryParse(User.max_time_per_appointments.ToString(), out _))
            {
                ErrorMessage = "Please enter a valid numeric value for max time in one appointment.";
                return;
            }

            if (User.break_between_two_appointments == null || !int.TryParse(User.break_between_two_appointments.ToString(), out _))
            {
                ErrorMessage = "Please enter a valid numeric value for the break between two appointments.";
                return;
            }


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE User_Table SET work_start_time = @work_start_time, work_end_time = @work_end_time, " +
                                     "weekly_work_days = @weekly_work_days, max_time_per_appointments = @max_time_per_appointments," +
                                     "break_between_two_appointments = @break_between_two_appointments, doctor_profile_complete = 1 WHERE id = @UserId;";

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", UserId);
                    command.Parameters.AddWithValue("@work_start_time", User.work_start_time);
                    command.Parameters.AddWithValue("@work_end_time", User.work_end_time);
                    command.Parameters.AddWithValue("@weekly_work_days", string.Join(",", User.weekly_work_days)); 
                    command.Parameters.AddWithValue("@max_time_per_appointments", User.max_time_per_appointments);
                    command.Parameters.AddWithValue("@break_between_two_appointments", User.break_between_two_appointments);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        SuccessMessage = "Doctor working information updated successfully.";
                    }
                    else
                    {
                        ErrorMessage = "Failed to update doctor working information.";
                    }
                }
            }
        }



    }


}

