using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HealthConnect.Pages.User
{
    public class Docter_ProfileModel : PageModel
    {
        private readonly ILogger<Explore_doctorsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public Docter_ProfileModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, ILogger<Explore_doctorsModel> logger, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        [BindProperty]
        public User_Table User { get; set; }

        public List<User_Table> User_list = new List<User_Table>();

        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string CurrencyCode { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }


        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();
        public List<Doctor_Feedback> Doctor_Feedbacks = new List<Doctor_Feedback>();


        [BindProperty] public Doctor_Feedback doctorFeedback { get; set; }

        public List<Doctor_Feedback> Doctor_Feedback = new List<Doctor_Feedback>();


        [BindProperty] public Doctor_Feedback doctorReport { get; set; }

        public List<Doctor_Feedback> Doctor_Report = new List<Doctor_Feedback>();

        [BindProperty]
        public Star_Rating starRating { get; set; } = new Star_Rating();

        public List<Star_Rating> Star_RatingList = new List<Star_Rating>();
        public double AverageRating { get; set; }
        public int? ExistingRating { get; set; }
        public int TotalFeedbackCount { get; set; }


        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");

            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                OnGetLoginUserData();
            }

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
                                doctor_type_id = reader.GetInt32(0),

                                type_of_doctor = reader.GetString(1),
                            });
                        }
                    }
                }
            }

            int? doctorId = null;

            if (Request.Query.ContainsKey("doctor_id") && int.TryParse(Request.Query["doctor_id"], out int queryDoctorId))
            {
                doctorId = queryDoctorId;
            }
            else
            {
                int? sessionDoctorId = HttpContext.Session.GetInt32("DoctorId");
                if (sessionDoctorId.HasValue)
                {
                    doctorId = sessionDoctorId.Value;
                }
            }

            if (doctorId.HasValue)
            {
                GetDoctorsList(doctorId.Value);       
                OnGetStarRating(doctorId.Value);
                OnGetGlobalStarRating(doctorId.Value); 
                
            }
            OnGetCountFeedbacks();
            GetDoctorFeedback();

            return Page();
        }
        private void OnGetLoginUserData()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FirstName = reader["first_name"].ToString();
                            LastName = reader["last_name"].ToString();
                            Email = reader["email"].ToString();
                            ProfilePic = reader["profile_pic"].ToString();
                            Role = reader["role"].ToString();
                            Country = reader["country"].ToString();
                            State = reader["state"].ToString();
                            City = reader["city"].ToString();
                            CurrencyCode = reader["currency_code"].ToString();
                            UserId = (int?)reader["id"];
                        }
                    }
                    con.Close();
                }
            }
        }
        private void GetDoctorsList(int doctor_id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT u.id, u.first_name, u.last_name, u.email, u.mobil_no, 
            u.dob, u.House_number_and_Street_name, u.country, u.city, 
            u.state, u.pincode, u.gender, u.role, u.password, 
            u.profile_pic, u.doctore_medical_license_photo, u.medical_registration_no, 
            u.state_medical_council, u.year_of_registration, u.doctore_experience, 
            u.hospital_or_clinic, u.doctor_qualifications, 
            t.type_of_doctor AS doctor_type, 
            u.languages_spoken, u.clinic_or_hospital_address, u.on_site_consultation_fee, 
            u.doctor_profile_complete, u.account_create_date, u.block, u.isactive, 
            u.mobail_verifie, u.auth_token, u.medicine_type, 
            u.currency_code, u.video_call_consultation_fee, 
            STRING_AGG(ds.doctor_specialitis, ', ') AS doctor_specialitis_list, 
            u.work_start_time, u.work_end_time, u.weekly_work_days, 
            u.max_time_per_appointments, u.break_between_two_appointments
                FROM User_Table u
                LEFT JOIN Types_of_Doctor t ON u.doctor_type = t.doctor_type_id
                LEFT JOIN Doctor_Specialitis ds 
                    ON ds.doctor_specialitis_id IN (SELECT value FROM STRING_SPLIT(u.doctor_specialitis, ','))
                WHERE u.role = 'Doctor' AND u.doctor_profile_complete = 1 AND u.isactive = 1 AND u.id = @doctor_id
                GROUP BY u.id, u.first_name, u.last_name, u.email, u.mobil_no, 
            u.dob, u.House_number_and_Street_name, u.country, u.city, 
            u.state, u.pincode, u.gender, u.role, u.password, 
            u.profile_pic, u.doctore_medical_license_photo, u.medical_registration_no, 
            u.state_medical_council, u.year_of_registration, u.doctore_experience, 
            u.hospital_or_clinic, u.doctor_qualifications, t.type_of_doctor, 
            u.languages_spoken, u.clinic_or_hospital_address, u.on_site_consultation_fee, 
            u.doctor_profile_complete, u.account_create_date, u.block, u.isactive, 
            u.mobail_verifie, u.auth_token, u.medicine_type, 
            u.currency_code, u.video_call_consultation_fee, u.work_start_time, u.work_end_time, u.weekly_work_days, 
            u.max_time_per_appointments, u.break_between_two_appointments";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@doctor_id", doctor_id);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentYear = DateTime.Now.Year;
                            int doctorExperienceYear = reader.IsDBNull(19) ? 0 : int.Parse(reader.GetString(19));
                            int experienceInYears = doctorExperienceYear == 0 ? 0 : (currentYear - doctorExperienceYear) - 2;

                            var userData = new User_Table
                            {
                                id = reader.GetInt32(0),
                                first_name = reader.GetString(1),
                                last_name = reader.GetString(2),
                                email = reader.GetString(3),
                                mobil_no = reader.IsDBNull(4) ? null : reader.GetString(4),
                                dob = reader.IsDBNull(5) ? null : reader.GetString(5),
                                House_number_and_Street_name = reader.IsDBNull(6) ? null : reader.GetString(6),
                                country = reader.IsDBNull(7) ? null : reader.GetString(7),
                                city = reader.IsDBNull(8) ? null : reader.GetString(8),
                                state = reader.IsDBNull(9) ? null : reader.GetString(9),
                                pincode = reader.IsDBNull(10) ? null : reader.GetString(10),
                                gender = reader.IsDBNull(11) ? null : reader.GetString(11),
                                role = reader.IsDBNull(12) ? null : reader.GetString(12),
                                password = reader.IsDBNull(13) ? null : reader.GetString(13),
                                profile_pic = reader.IsDBNull(14) ? null : reader.GetString(14),
                                doctore_medical_license_photo = reader.IsDBNull(15) ? null : reader.GetString(15),
                                medical_registration_no = reader.IsDBNull(16) ? null : reader.GetString(16),
                                state_medical_council = reader.IsDBNull(17) ? null : reader.GetString(17),
                                year_of_registration = reader.IsDBNull(18) ? null : reader.GetString(18),
                                doctore_experience = experienceInYears.ToString(),
                                hospital_or_clinic = reader.IsDBNull(20) ? null : reader.GetString(20),
                                doctor_qualifications = reader.IsDBNull(21) ? null : reader.GetString(21),
                                doctor_type = reader.IsDBNull(22) ? null : reader.GetString(22),
                                languages_spoken = reader.IsDBNull(23) ? null : reader.GetString(23),
                                clinic_or_hospital_address = reader.IsDBNull(24) ? null : reader.GetString(24),
                                on_site_consultation_fee = reader.IsDBNull(25) ? null : reader.GetString(25),
                                doctor_profile_complete = reader.GetBoolean(26),
                                account_create_date = reader.GetDateTime(27),
                                block = reader.IsDBNull(28) ? (bool?)null : reader.GetBoolean(28),
                                isactive = reader.GetBoolean(29),
                                mobail_verifie = reader.IsDBNull(30) ? (bool?)null : reader.GetBoolean(30),
                                auth_token = reader.IsDBNull(31) ? null : reader.GetString(31),
                                medicine_type = reader.IsDBNull(32) ? null : reader.GetString(32),
                                currency_code = reader.IsDBNull(33) ? null : reader.GetString(33),
                                video_call_consultation_fee = reader.IsDBNull(34) ? null : reader.GetString(34),
                                doctor_specialitis = reader.IsDBNull(35) ? null : reader.GetString(35),
                                work_start_time = reader.IsDBNull(36) ? null : reader.GetString(36),
                                work_end_time = reader.IsDBNull(37) ? null : reader.GetString(37),
                                weekly_work_days = reader.IsDBNull(38) ? new List<string>() : reader.GetString(38).Split(',').ToList(),
                                max_time_per_appointments = reader.IsDBNull(39) ? null : reader.GetString(39),
                                break_between_two_appointments = reader.IsDBNull(40) ? null : reader.GetString(40)
                            };
                            User_list.Add(userData);
                        }
                    }
                }
            }
        }
        private void GetDoctorFeedback()
        {
            int? doctorId = null;

            if (Request.Query.ContainsKey("doctor_id"))
            {
                doctorId = int.TryParse(Request.Query["doctor_id"], out int id) ? id : (int?)null;
            }

            if (!doctorId.HasValue)
            {
                Doctor_Feedbacks = new List<Doctor_Feedback>(); // Initialize with an empty list
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT 
            df.doctor_feedback_id, 
            df.user_id, 
            df.doctor_id, 
            df.feedback_message, 
            u.first_name, 
            u.last_name, 
            u.profile_pic
        FROM Doctor_Feedback df
        JOIN User_Table u ON df.user_id = u.id
        WHERE df.doctor_id = @doctor_id";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@doctor_id", doctorId.Value);
                    connection.Open();

                    Doctor_Feedbacks = new List<Doctor_Feedback>();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var feedback = new Doctor_Feedback
                            {
                                doctor_feedback_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                doctor_id = reader.GetInt32(2),
                                feedback_message = reader.GetString(3),
                                User = new User_Table
                                {
                                    first_name = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    last_name = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                    profile_pic = reader.IsDBNull(6) ? "" : reader.GetString(6)
                                }
                            };
                            Doctor_Feedbacks.Add(feedback);
                        }
                    }
                }
            }
        }
       

        public void OnGetStarRating(int doctor_id)
        {
            UserId = HttpContext.Session.GetInt32("Id");

            if (!UserId.HasValue) return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT rating FROM Star_Rating WHERE user_id = @UserId AND doctor_id = @DoctorId";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId.Value);
                    cmd.Parameters.AddWithValue("@DoctorId", doctor_id);

                    object result = cmd.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int rating))
                    {
                        ExistingRating = rating;
                    }
                }
            }
        }
        public void OnGetGlobalStarRating(int doctor_id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT AVG(CAST(rating AS FLOAT)) FROM Star_Rating WHERE doctor_id = @DoctorId";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@DoctorId", doctor_id);

                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        AverageRating = Math.Round(Convert.ToDouble(result), 1);
                    }
                }
            }
        }
        private void OnGetCountFeedbacks()
        {
            int? doctorId = null;

            if (Request.Query.ContainsKey("doctor_id"))
            {
                doctorId = int.TryParse(Request.Query["doctor_id"], out int id) ? id : (int?)null;
            }

            if (!doctorId.HasValue)
            {
                Doctor_Feedbacks = new List<Doctor_Feedback>();
                return;
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Doctor_Feedback WHERE doctor_id = @doctor_id"; 

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@doctor_id", doctorId);  

                    object result = cmd.ExecuteScalar();
                    TotalFeedbackCount = (result != DBNull.Value) ? Convert.ToInt32(result) : 0;
                }
            }
        }




        public async Task<IActionResult> OnPost()
        {
            string action = Request.Form["action"];
            if (action == "feedback")
            {
                return await OnPostGiveFeedback();
            }
            else if (action == "report")
            {
                return await OnPostGiveReport();
            }
            else if (action == "Rating")
            {
                return OnPostGiveStarRating();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostGiveFeedback()
        {
            string userId = Request.Form["user_id"];
            string doctorId = Request.Form["doctor_id"];
            string feedbackMessage = Request.Form["feedback_message"];

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(doctorId) || string.IsNullOrEmpty(feedbackMessage))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return Redirect($"/User/Doctor_Profile?doctor_id={doctorId}");
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM Doctor_Feedback WHERE user_id = @user_id AND doctor_id = @doctor_id";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@user_id", userId);
                    checkCommand.Parameters.AddWithValue("@doctor_id", doctorId);
                    int existingFeedbackCount = (int)await checkCommand.ExecuteScalarAsync();

                    if (existingFeedbackCount > 0)
                    {
                        string updateQuery = "UPDATE Doctor_Feedback SET feedback_message = @feedback_message WHERE user_id = @user_id AND doctor_id = @doctor_id";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@user_id", userId);
                            updateCommand.Parameters.AddWithValue("@doctor_id", doctorId);
                            updateCommand.Parameters.AddWithValue("@feedback_message", feedbackMessage);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                        TempData["SuccessMessage"] = "Feedback updated successfully.";
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO Doctor_Feedback (user_id, doctor_id, feedback_message) VALUES (@user_id, @doctor_id, @feedback_message)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@user_id", userId);
                            insertCommand.Parameters.AddWithValue("@doctor_id", doctorId);
                            insertCommand.Parameters.AddWithValue("@feedback_message", feedbackMessage);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                        TempData["SuccessMessage"] = "Feedback submitted successfully.";
                    }
                }
            }

            string userEmail = "";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string userEmailQuery = "SELECT email FROM User_Table WHERE id = @UserId";
                using (SqlCommand userEmailCommand = new SqlCommand(userEmailQuery, connection))
                {
                    userEmailCommand.Parameters.AddWithValue("@UserId", userId);
                    userEmail = (await userEmailCommand.ExecuteScalarAsync())?.ToString();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subjectUser = "Feedback Submitted";
                string bodyUser = $"Dear User, your feedback has been submitted:\n\n{feedbackMessage}.";
                await _emailService.SendEmailAsync(userEmail, subjectUser, bodyUser);
            }

            return Redirect($"/User/Doctor_Profile?doctor_id={doctorId}");
        }
        public async Task<IActionResult> OnPostGiveReport()
        {

            string userId = Request.Form["user_id"];
            string doctorId = Request.Form["doctor_id"];
            string reportMessage = Request.Form["report_message"];


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = "SELECT COUNT(*) FROM Doctor_Report WHERE user_id = @user_id AND doctor_id = @doctor_id";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@user_id", userId);
                    checkCommand.Parameters.AddWithValue("@doctor_id", doctorId);
                    int existingReportCount = (int)await checkCommand.ExecuteScalarAsync();

                    if (existingReportCount > 0)
                    {
                        string updateQuery = "UPDATE Doctor_Report SET report_message = @report_message WHERE user_id = @user_id AND doctor_id = @doctor_id";
                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@user_id", userId);
                            updateCommand.Parameters.AddWithValue("@doctor_id", doctorId);
                            updateCommand.Parameters.AddWithValue("@report_message", reportMessage);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                        TempData["SuccessMessage"] = "Report updated successfully.";
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO Doctor_Report (user_id, doctor_id, report_message) VALUES (@user_id, @doctor_id, @report_message)";
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@user_id", userId);
                            insertCommand.Parameters.AddWithValue("@doctor_id", doctorId);
                            insertCommand.Parameters.AddWithValue("@report_message", reportMessage);
                            await insertCommand.ExecuteNonQueryAsync();
                        }
                        TempData["SuccessMessage"] = "Report submitted successfully.";
                    }
                }
            }

            string userEmail = "";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string userEmailQuery = "SELECT email FROM User_Table WHERE id = @UserId";
                using (SqlCommand userEmailCommand = new SqlCommand(userEmailQuery, connection))
                {
                    userEmailCommand.Parameters.AddWithValue("@UserId", userId);
                    userEmail = (await userEmailCommand.ExecuteScalarAsync())?.ToString();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subjectUser = "Report Submitted";
                string bodyUser = $"Dear User, your report has been submitted:\n\n{reportMessage}.";
                await _emailService.SendEmailAsync(userEmail, subjectUser, bodyUser);
            }

            return Redirect($"/User/Doctor_Profile?doctor_id={doctorId}");


        }

        public IActionResult OnPostGiveStarRating()
        {
            string userId = Request.Form["user_id"];
            string doctorId = Request.Form["doctor_id"];
            string rating = Request.Form["rating"];

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be signed in to rate a doctor.";
                return Redirect("/User/Sign_in");
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Star_Rating (user_id, doctor_id, rating) VALUES (@UserId, @DoctorId, @Rating)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    cmd.Parameters.AddWithValue("@Rating", rating);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Rating submitted successfully!";
            return Redirect($"/User/Doctor_Profile?doctor_id={doctorId}");
        }


    }
}
