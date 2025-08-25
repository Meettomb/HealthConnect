using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Medicine_list_management.Medicine_Main_Category_manage
{
    public class Medicine_Main_Category_EditModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Medicine_Main_Category MedicineMainCategory { get; set; } = new Medicine_Main_Category();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Medicine_Main_Category_EditModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int medicine_main_category_id)
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
                string query = "SELECT * FROM Medicine_Main_Category WHERE medicine_main_category_id = @medicine_main_category_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_main_category_id", medicine_main_category_id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            MedicineMainCategory = new Medicine_Main_Category
                            {
                                medicine_main_category_id = reader.GetInt32(0),
                                medicine_main_category_name = reader.GetString(1),
                            };
                        }
                    }

                    connection.Close();
                }
            }

            if (MedicineMainCategory == null)
            {
                ErrorMessage = "Medicine Main Category not found.";
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
                string query = "UPDATE Medicine_Main_Category SET medicine_main_category_name = @medicine_main_category_name WHERE medicine_main_category_id = @medicine_main_category_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_main_category_name", MedicineMainCategory.medicine_main_category_name);
                    command.Parameters.AddWithValue("@medicine_main_category_id", MedicineMainCategory.medicine_main_category_id);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Medicine list updated successfully";
                        return RedirectToPage("/Admin/Medicine_list_management/Medicine_Main_Category_manage/Medicine_Main_Category");
                    }
                    else
                    {
                        ErrorMessage = "Error updating the medicine list.";
                    }
                }
            }

            return Page();
        }

    }
}
