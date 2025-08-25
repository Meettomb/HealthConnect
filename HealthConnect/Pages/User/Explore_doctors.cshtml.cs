using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace HealthConnect.Pages.User
{
    public class Explore_doctorsModel : PageModel
    {
        private readonly ILogger<Explore_doctorsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public Explore_doctorsModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, ILogger<Explore_doctorsModel> logger, IConfiguration configuration)
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
        public double AverageRating { get; set; }
        public int? ExistingRating { get; set; }
        public int TotalFeedbackCount { get; set; }



        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

        public List<Doctor_Feedback> Doctor_Feedbacks = new List<Doctor_Feedback>();

        [BindProperty] public Appointments Appointments { get; set; }



        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");

            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
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

            GetDoctorsList("Doctor"); 
           

            return Page();
        }

        private void GetDoctorsList(string role)
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
WHERE u.role = @Role AND u.doctor_profile_complete = 1 AND u.isactive = 1
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
                    cmd.Parameters.AddWithValue("@Role", role);
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

                            using (SqlConnection feedbackConnection = new SqlConnection(_connectionString))
                            {
                                feedbackConnection.Open();

                                string feedbackQuery = "SELECT COUNT(*) FROM Doctor_Feedback WHERE doctor_id = @DoctorId";
                                using (SqlCommand feedbackCmd = new SqlCommand(feedbackQuery, feedbackConnection))
                                {
                                    feedbackCmd.Parameters.AddWithValue("@DoctorId", userData.id);

                                    object feedbackResult = feedbackCmd.ExecuteScalar();
                                    userData.TotalFeedbackCount = (feedbackResult != DBNull.Value) ? Convert.ToInt32(feedbackResult) : 0;
                                }

                                string ratingQuery = "SELECT AVG(CAST(rating AS FLOAT)) FROM Star_Rating WHERE doctor_id = @DoctorId";
                                using (SqlCommand ratingCmd = new SqlCommand(ratingQuery, feedbackConnection))
                                {
                                    ratingCmd.Parameters.AddWithValue("@DoctorId", userData.id);

                                    object ratingResult = ratingCmd.ExecuteScalar();
                                    if (ratingResult != DBNull.Value && ratingResult != null)
                                    {
                                        userData.AverageRating = Math.Round(Convert.ToDouble(ratingResult), 1);
                                    }
                                }
                            }

                            User_list.Add(userData);
                        }
                    }
                }
            }
        }






        public async Task<IActionResult> OnPost()
        {
            await OnPostBookAppointmentAsync();
            return RedirectToPage();
        }

        public async Task OnPostBookAppointmentAsync()
        {
            string userEmail = null;
            string doctorEmail = null;
            int rowEffect = 0;


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string checkQuery = @"
            SELECT COUNT(*) FROM Appointments 
            WHERE doctor_id = @DoctorId 
            AND appointment_date = @AppointmentDate 
            AND time_slot = @TimeSlot 
            AND appointment_approve = 1";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@DoctorId", Appointments.doctor_id);
                    checkCmd.Parameters.AddWithValue("@AppointmentDate", Appointments.appointment_date);
                    checkCmd.Parameters.AddWithValue("@TimeSlot", Appointments.time_slot);

                    int existingCount = (int)await checkCmd.ExecuteScalarAsync();

                    if (existingCount > 0)
                    {
                        TempData["ErrorMessage"] = "This time slot is already booked. Please choose another slot.";
                        return;
                    }
                }
                string insertQuery = @"
            INSERT INTO Appointments (user_id, doctor_id, appointment_type, time_slot, appointment_date, appointment_approve, book_date_time, booking_user_role, problem, appointment_cancel) 
            VALUES (@UserId, @DoctorId, @AppointmentType, @TimeSlot, @AppointmentDate, @AppointmentApprove, @book_date_time, @booking_user_role, @problem, @appointment_cancel)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", Appointments.user_id);
                    command.Parameters.AddWithValue("@DoctorId", Appointments.doctor_id);
                    command.Parameters.AddWithValue("@AppointmentType", Appointments.appointment_type);
                    command.Parameters.AddWithValue("@TimeSlot", Appointments.time_slot);
                    command.Parameters.AddWithValue("@AppointmentDate", Appointments.appointment_date);
                    command.Parameters.AddWithValue("@AppointmentApprove", false);
                    command.Parameters.AddWithValue("@book_date_time", DateTime.Now);
                    command.Parameters.AddWithValue("@booking_user_role", Appointments.booking_user_role);
                    command.Parameters.AddWithValue("@problem", string.IsNullOrEmpty(Appointments.problem) ? (object)DBNull.Value : Appointments.problem);
                    command.Parameters.AddWithValue("@appointment_cancel", false);


                    rowEffect = await command.ExecuteNonQueryAsync();
                }

                if (rowEffect > 0)
                {
                    TempData["SuccessMessage"] = "Appointment booked successfully!";

                    string emailQuery = "SELECT id, email FROM User_Table WHERE id = @UserId OR id = @DoctorId";
                    using (SqlCommand emailCmd = new SqlCommand(emailQuery, connection))
                    {
                        emailCmd.Parameters.AddWithValue("@UserId", Appointments.user_id);
                        emailCmd.Parameters.AddWithValue("@DoctorId", Appointments.doctor_id);

                        using (SqlDataReader reader = await emailCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string email = reader["email"].ToString();
                                int fetchedId = Convert.ToInt32(reader["id"]);

                                if (fetchedId == Appointments.user_id)
                                {
                                    userEmail = email;
                                }
                                else if (fetchedId == Appointments.doctor_id)
                                {
                                    doctorEmail = email;
                                }
                            }
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to book appointment. Please try again.";
                }
            }
            if (!string.IsNullOrEmpty(userEmail))
            {
                string subject = "Appointment Confirmation";
                string body = $"Dear User,\n\nYou have applied for an appointment on {Appointments.appointment_date:yyyy-MM-dd} at {Appointments.time_slot}.\n\nWe will notify you once it is confirmed.\n\nThank you.";

                await _emailService.SendEmailAsync(userEmail, subject, body);
                Console.WriteLine("User email sent.");
            }

            if (!string.IsNullOrEmpty(doctorEmail))
            {
                string subject = "New Appointment Scheduled";
                string body = $"Dear Doctor, a new appointment has been booked by a patient. " +
                              $"Appointment Type: {Appointments.appointment_type} " +
                              $"Time: {Appointments.appointment_date} at {Appointments.time_slot}.";
                await _emailService.SendEmailAsync(doctorEmail, subject, body);
                Console.WriteLine("Doctor email sent.");
            }
        }


    }
}
