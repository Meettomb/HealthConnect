using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Doctor_list_management.Doctor_types_manage
{
    public class Doctor_typesModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();



        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Doctor_typesModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

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

            return Page();
        }


        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(TypesOfDoctor.type_of_doctor))
            {
                ModelState.AddModelError(string.Empty, "Doctor type cannot be empty.");
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Types_of_Doctor (type_of_doctor) VALUES (@Type_of_doctor)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Type_of_doctor", TypesOfDoctor.type_of_doctor);
                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Doctor type added successfully.";
            return RedirectToPage("/Admin/Doctor_list_management/Doctor_types_manage/Doctor_types");
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
                string query = "DELETE FROM Types_of_Doctor WHERE doctor_type_id = @doctor_type_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctor_type_id", id);
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Doctor type deleted successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error deleting doctor type.");
                    }
                }
            }

            return RedirectToPage("/Admin/Doctor_list_management/Doctor_types_manage/Doctor_types");
        }

    }
}
