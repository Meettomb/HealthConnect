using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Medicine_list_management.Medicine_Sub_Category_manage
{
    public class Medicine_Sub_Category_EditModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Medicine_Main_Category MedicineMainCategory { get; set; } = new Medicine_Main_Category();

        public List<Medicine_Main_Category> Medicine_Main_Category { get; set; } = new List<Medicine_Main_Category>();

        [BindProperty]
        public Medicine_Sub_Category MedicineSubCategory { get; set; } = new Medicine_Sub_Category();

        public List<Medicine_Sub_Category> Medicine_Sub_Category { get; set; } = new List<Medicine_Sub_Category>();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Medicine_Sub_Category_EditModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int medicine_sub_category_id)
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
                string query = "SELECT * FROM Medicine_Main_Category";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medicine_Main_Category.Add(new Medicine_Main_Category
                            {
                                medicine_main_category_id = reader.GetInt32(0),
                                medicine_main_category_name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Medicine_Sub_Category WHERE medicine_sub_category_id = @medicine_sub_category_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_sub_category_id", medicine_sub_category_id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            MedicineSubCategory = new Medicine_Sub_Category
                            {
                                medicine_sub_category_id = reader.GetInt32(0),
                                medicine_main_category_id = reader.GetInt32(1),
                                medicine_sub_category_name = reader.GetString(2)
                            };
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Medicine sub category not found.";
                            return RedirectToPage("/Error");
                        }
                    }
                }
            }

            if (MedicineSubCategory == null)
            {
                ErrorMessage = "Medicine sub category not found.";
                return RedirectToPage("/Error");
            }

            return Page();
        }



        public IActionResult OnPost()
        {


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Medicine_Sub_Category SET medicine_sub_category_name = @medicine_sub_category_name, medicine_main_category_id = @medicine_main_category_id WHERE medicine_sub_category_id = @medicine_sub_category_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_sub_category_name", MedicineSubCategory.medicine_sub_category_name);
                    command.Parameters.AddWithValue("@medicine_main_category_id", MedicineSubCategory.medicine_main_category_id);
                    command.Parameters.AddWithValue("@medicine_sub_category_id", MedicineSubCategory.medicine_sub_category_id);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Medicine sub category updated successfully.";
                        return RedirectToPage("/Admin/Medicine_list_management/Medicine_Sub_Category_manage/Medicine_Sub_Category");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error updating Medicine sub category.";
                        return Page();
                    }
                }
            }
        }

    }
}
