using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Doctor_list_management.Doctor_specialties_manage
{
    public class Doctor_specialtiesModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Doctor_specialtiesModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

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
                                FirstName = reader["first_name"]?.ToString();
                                LastName = reader["last_name"]?.ToString();
                                ProfilePic = reader["profile_pic"]?.ToString();
                                Role = reader["role"]?.ToString();
                                UserId = reader["id"] as int?;
                            }
                            else
                            {
                                ErrorMessage = "User not found.";
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


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Types_of_Doctor";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            Types_of_Doctor doctorType = new Types_of_Doctor();
                            doctorType.doctor_type_id = reader.GetInt32(0);
                            doctorType.type_of_doctor = reader.GetString(1);

                            Types_of_doctor.Add(doctorType);
                        }
                    }
                    connection.Close();
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT 
            DS.doctor_specialitis_id,
            DS.doctor_type_id,
            DS.doctor_specialitis,
            TD.type_of_doctor
        FROM 
            Doctor_Specialitis DS
        JOIN 
            Types_of_Doctor TD
        ON 
            DS.doctor_type_id = TD.doctor_type_id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Doctor_Specialitis doctorSpecialitis = new Doctor_Specialitis
                            {
                                doctor_specialitis_id = reader.GetInt32(0),
                                doctor_type_id = reader.GetInt32(1),
                                doctor_specialitis = reader.GetString(2),
                                Types_of_Doctor = new Types_of_Doctor
                                {
                                    doctor_type_id = reader.GetInt32(1),
                                    type_of_doctor = reader.GetString(3)
                                }
                            };

                            doctorSpecialitiesList.Add(doctorSpecialitis);
                        }
                    }
                    connection.Close();
                }
            }


            return Page();
        }


        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(SpecialitisOfDoctor.doctor_specialitis))
            {
                ModelState.AddModelError(nameof(SpecialitisOfDoctor.doctor_specialitis), "Doctor specialization cannot be empty.");
                TempData["ErrorMessage"] = "Doctor specialization cannot be empty.";
                return RedirectToPage();
            }

            if (SpecialitisOfDoctor.doctor_type_id == 0)
            {
                ModelState.AddModelError(nameof(SpecialitisOfDoctor.doctor_type_id), "Doctor type cannot be empty.");
                TempData["ErrorMessage"] = "Doctor type cannot be empty.";
                return RedirectToPage();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Doctor_Specialitis (doctor_type_id, doctor_specialitis) VALUES (@Doctor_type_id, @Doctor_specialitis)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Doctor_type_id", SpecialitisOfDoctor.doctor_type_id);
                    command.Parameters.AddWithValue("@Doctor_specialitis", SpecialitisOfDoctor.doctor_specialitis);
                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Doctor specialization added successfully.";
            return RedirectToPage("/Admin/Doctor_list_management/Doctor_specialties_manage/Doctor_specialties");
        }


        public IActionResult OnPostDelete(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid doctor type ID.");
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Doctor_Specialitis WHERE doctor_specialitis_id = @doctor_specialitis_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctor_specialitis_id", id);
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Doctor specialiti deleted successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error deleting doctor type.");
                    }
                }
            }

            return RedirectToPage("/Admin/Doctor_list_management/Doctor_specialties_manage/Doctor_specialties");
        }


    }
}