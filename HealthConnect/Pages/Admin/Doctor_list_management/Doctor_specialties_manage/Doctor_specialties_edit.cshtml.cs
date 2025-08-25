using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Doctor_list_management
{
    public class Doctor_specialties_editModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Doctor_specialties_editModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int doctor_specialitis_id)
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
                            Types_of_doctor.Add(new Types_of_Doctor
                            {
                                doctor_type_id = reader.GetInt32(0),
                                type_of_doctor = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            // Fetch Doctor Specialitis
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Doctor_Specialitis WHERE doctor_specialitis_id = @doctor_specialitis_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctor_specialitis_id", doctor_specialitis_id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            SpecialitisOfDoctor = new Doctor_Specialitis
                            {
                                doctor_specialitis_id = reader.GetInt32(0),
                                doctor_type_id = reader.GetInt32(1),
                                doctor_specialitis = reader.GetString(2)
                            };
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Doctor specialty not found.";
                            return RedirectToPage("/Error");
                        }
                    }
                }
            }

            if (SpecialitisOfDoctor == null)
            {
                ErrorMessage = "Doctor specialitis not found.";
                return RedirectToPage("/Error");
            }

            return Page();
        }



        public IActionResult OnPost()
        {


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Doctor_Specialitis SET doctor_specialitis = @doctor_specialitis, doctor_type_id = @doctor_type_id WHERE doctor_specialitis_id = @doctor_specialitis_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctor_specialitis", SpecialitisOfDoctor.doctor_specialitis);
                    command.Parameters.AddWithValue("@doctor_type_id", SpecialitisOfDoctor.doctor_type_id);
                    command.Parameters.AddWithValue("@doctor_specialitis_id", SpecialitisOfDoctor.doctor_specialitis_id);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Doctor specialty updated successfully.";
                        return RedirectToPage("/Admin/Doctor_list_management/Doctor_specialties_manage/Doctor_specialties");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error updating doctor specialty.";
                        return Page();
                    }
                }
            }
        }

    }
}