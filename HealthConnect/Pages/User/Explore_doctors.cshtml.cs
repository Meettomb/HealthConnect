using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HealthConnect.Pages.User
{
    public class Explore_doctorsModel : PageModel
    {
        private readonly ILogger<Explore_doctorsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        [BindProperty]
        public User_Table User { get; set; }

        public List<User_Table> User_list = new List<User_Table>();

        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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




        public Explore_doctorsModel(ILogger<Explore_doctorsModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");

            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                GetCurrentUserDetails(UserId.Value);
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

        private void GetCurrentUserDetails(int userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
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
                            CurrencyCode = reader["currency_code"].ToString();
                            UserId = (int?)reader["id"];
                        }
                    }
                    con.Close();
                }
            }
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
       u.hospital_or_clinic, u.doctor_qualifications, u.doctor_type, 
       u.languages_spoken, u.clinic_or_hospital_address, u.on_site_consultation_fee, 
       u.account_approve, u.account_create_date, u.block, u.isactive, 
       u.mobail_verifie, u.auth_token, u.medicine_type, 
       u.currency_code, u.video_call_consultation_fee,
       STRING_AGG(ds.doctor_specialitis, ', ') AS doctor_specialitis_list 
FROM User_Table u
LEFT JOIN Doctor_Specialitis ds 
    ON ds.doctor_specialitis_id IN (SELECT value FROM STRING_SPLIT(u.doctor_specialitis, ','))
WHERE u.role = @Role
GROUP BY u.id, u.first_name, u.last_name, u.email, u.mobil_no, 
         u.dob, u.House_number_and_Street_name, u.country, u.city, 
         u.state, u.pincode, u.gender, u.role, u.password, 
         u.profile_pic, u.doctore_medical_license_photo, u.medical_registration_no, 
         u.state_medical_council, u.year_of_registration, u.doctore_experience, 
         u.hospital_or_clinic, u.doctor_qualifications, u.doctor_type, 
         u.languages_spoken, u.clinic_or_hospital_address, u.on_site_consultation_fee, 
         u.account_approve, u.account_create_date, u.block, u.isactive, 
         u.mobail_verifie, u.auth_token, u.medicine_type, 
         u.currency_code, u.video_call_consultation_fee";


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
                                account_approve = reader.GetBoolean(26),
                                account_create_date = reader.GetDateTime(27),
                                block = reader.IsDBNull(28) ? (bool?)null : reader.GetBoolean(28),
                                isactive = reader.GetBoolean(29),
                                mobail_verifie = reader.IsDBNull(30) ? (bool?)null : reader.GetBoolean(30),
                                auth_token = reader.IsDBNull(31) ? null : reader.GetString(31),
                                medicine_type = reader.IsDBNull(32) ? null : reader.GetString(32),
                                currency_code = reader.IsDBNull(33) ? null : reader.GetString(33),
                                video_call_consultation_fee = reader.IsDBNull(34) ? null : reader.GetString(34),
                                doctor_specialitis = reader.IsDBNull(35) ? null : reader.GetString(35)
                            };
                            User_list.Add(userData);
                        }
                    }
                }
            }
        }

    }
}
