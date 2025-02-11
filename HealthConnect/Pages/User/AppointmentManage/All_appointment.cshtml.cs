using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthConnect.Pages.User.AppointmentManage
{
    public class All_appointmentModel : PageModel
    {
        private readonly ILogger<Explore_doctorsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public All_appointmentModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, ILogger<Explore_doctorsModel> logger, IConfiguration configuration)
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


        [BindProperty] public Appointments Appointments { get; set; }
        public List<Appointments> AppointmentsList { get; set; } = new List<Appointments>();



        public void OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");

            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                OnGetUserDetails();
            }
            OnGetDoctorSpecialities();
            OnGetDoctorTypes();
            OnGetAppointments();

        }


        public void OnGetUserDetails()
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

        public void OnGetDoctorSpecialities()
        {
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
        }

        public void OnGetDoctorTypes()
        {
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
        }

        public void OnGetAppointments()
        {
            Role = HttpContext.Session.GetString("UserRole"); // Fetch role from session
            UserId = HttpContext.Session.GetInt32("Id"); // Fetch user ID from session

            if (string.IsNullOrEmpty(Role) || UserId == null)
            {
                Console.WriteLine("UserRole or UserId is missing from session!");
                return;
            }

            int loggedInUserId = UserId.Value; // Convert nullable int to int safely
            Console.WriteLine($"Logged in as {Role}, UserID: {loggedInUserId}");

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT A.appointment_id, A.user_id, A.doctor_id, A.appointment_type, 
                       A.time_slot, A.appointment_date, A.appointment_approve, A.book_date_time,
                       U.first_name AS user_first_name, U.last_name AS user_last_name, U.profile_pic AS user_profile_pic,
                       D.first_name AS doctor_first_name, D.last_name AS doctor_last_name, D.profile_pic AS doctor_profile_pic,
                        D.country AS doctor_country, D.state AS doctor_state, D.city AS doctor_city, D.year_of_registration AS doctor_year_of_registration,
                        D.doctore_experience AS doctore_experience, D.hospital_or_clinic AS doctor_hospital_or_clinic, D.clinic_or_hospital_address AS doctor_clinic_or_hospital_address,
                        D.on_site_consultation_fee AS doctor_on_site_consultation_fee, D.video_call_consultation_fee AS doctor_video_call_consultation_fee,
                        D.doctor_specialitis AS doctor_specialitis, D.languages_spoken AS doctor_languages_spoken, D.currency_code AS doctor_currency_code,
                        D.max_time_per_appointments AS doctor_max_time_per_appointments, D.break_between_two_appointments AS doctor_break_between_two_appointments,
                        D.email AS doctor_email
                FROM Appointments A
                INNER JOIN User_Table U ON A.user_id = U.id
                INNER JOIN User_Table D ON A.doctor_id = D.id
                WHERE A.user_id = @LoggedInUserId OR A.doctor_id = @LoggedInUserId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LoggedInUserId", loggedInUserId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var appointment = new Appointments
                            {
                                appointment_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                doctor_id = reader.GetInt32(2),
                                appointment_type = reader.GetString(3),
                                time_slot = reader.GetString(4),
                                appointment_date = reader.GetString(5),
                                appointment_approve = reader.GetBoolean(6),
                                book_date_time = reader.GetDateTime(7),
                                User = new User_Table(),
                                Doctor = new User_Table()
                            };

                            appointment.User.first_name = reader.IsDBNull(8) ? "" : reader.GetString(8);
                            appointment.User.last_name = reader.IsDBNull(9) ? "" : reader.GetString(9);
                            appointment.User.profile_pic = reader.IsDBNull(10) ? "" : reader.GetString(10);

                            appointment.Doctor.first_name = reader.IsDBNull(11) ? "" : reader.GetString(11);
                            appointment.Doctor.last_name = reader.IsDBNull(12) ? "" : reader.GetString(12);
                            appointment.Doctor.profile_pic = reader.IsDBNull(13) ? "" : reader.GetString(13);
                            appointment.Doctor.country = reader.IsDBNull(14) ? "" : reader.GetString(14);
                            appointment.Doctor.state = reader.IsDBNull(15) ? "" : reader.GetString(15);
                            appointment.Doctor.city = reader.IsDBNull(16) ? "" : reader.GetString(16);
                            appointment.Doctor.year_of_registration = reader.IsDBNull(17) ? "" : reader.GetString(17);
                            appointment.Doctor.doctore_experience = reader.IsDBNull(18) ? "" : reader.GetString(18);
                            appointment.Doctor.hospital_or_clinic = reader.IsDBNull(19) ? "" : reader.GetString(19);
                            appointment.Doctor.clinic_or_hospital_address = reader.IsDBNull(20) ? "" : reader.GetString(20);
                            appointment.Doctor.on_site_consultation_fee = reader.IsDBNull(21) ? "" : reader.GetString(21);
                            appointment.Doctor.video_call_consultation_fee = reader.IsDBNull(22) ? "" : reader.GetString(22);
                            appointment.Doctor.doctor_specialitis = reader.IsDBNull(23) ? "" : reader.GetString(23);
                            appointment.Doctor.languages_spoken = reader.IsDBNull(24) ? "" : reader.GetString(24);
                            appointment.Doctor.currency_code = reader.IsDBNull(25) ? "" : reader.GetString(25);
                            appointment.Doctor.max_time_per_appointments = reader.IsDBNull(26) ? "" : reader.GetString(26);
                            appointment.Doctor.break_between_two_appointments = reader.IsDBNull(27) ? "" : reader.GetString(27);
                            appointment.Doctor.email = reader.IsDBNull(28) ? "" : reader.GetString(28);



                            AppointmentsList.Add(appointment);
                        }
                    }
                }
            }

            Console.WriteLine($"Appointments found: {AppointmentsList.Count}");
            foreach (var app in AppointmentsList)
            {
                Console.WriteLine($"Appointment ID: {app.appointment_id}, User: {app.User?.first_name}, Doctor: {app.Doctor?.first_name}");
            }
        }

    }
}
