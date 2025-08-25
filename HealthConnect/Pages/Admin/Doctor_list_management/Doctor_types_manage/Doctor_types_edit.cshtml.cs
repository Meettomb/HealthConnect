using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace HealthConnect.Pages.Admin.Doctor_list_management.Doctor_types_manage
{
    public class Doctor_types_editModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Doctor_types_editModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int doctor_type_id)
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
                string query = "SELECT * FROM Types_of_Doctor WHERE doctor_type_id = @doctor_type_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctor_type_id", doctor_type_id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            TypesOfDoctor = new Types_of_Doctor
                            {
                                doctor_type_id = reader.GetInt32(0),
                                type_of_doctor = reader.GetString(1),
                            };
                        }
                    }

                    connection.Close();
                }
            }

            if (TypesOfDoctor == null)
            {
                ErrorMessage = "Doctor type not found.";
                return RedirectToPage("/Error");
            }

            return Page();
        }



        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Types_of_Doctor SET type_of_doctor = @type_of_doctor WHERE doctor_type_id = @doctor_type_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@type_of_doctor", TypesOfDoctor.type_of_doctor);
                    command.Parameters.AddWithValue("@doctor_type_id", TypesOfDoctor.doctor_type_id);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        return RedirectToPage("/Admin/Doctor_list_management/Doctor_types_manage/Doctor_types");
                    }
                    else
                    {
                        ErrorMessage = "Error updating the doctor type.";
                    }
                }
            }

            return Page();
        }


    }
}
