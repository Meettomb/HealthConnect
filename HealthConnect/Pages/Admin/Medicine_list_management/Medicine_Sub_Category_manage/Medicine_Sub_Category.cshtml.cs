using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Medicine_list_management.Medicine_Sub_Category_manage
{
    public class Medicine_Sub_CategoryModel : PageModel
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

        public Medicine_Sub_CategoryModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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
                string query = "SELECT * FROM Medicine_Main_Category";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            Medicine_Main_Category MedicineMainCategory = new Medicine_Main_Category();
                            MedicineMainCategory.medicine_main_category_id = reader.GetInt32(0);
                            MedicineMainCategory.medicine_main_category_name = reader.GetString(1);

                            Medicine_Main_Category.Add(MedicineMainCategory);
                        }
                    }
                    connection.Close();
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT 
            MS.medicine_sub_category_id,
            MS.medicine_main_category_id,
            MS.medicine_sub_category_name,
            MM.medicine_main_category_name
        FROM 
            Medicine_Sub_Category MS
        JOIN 
            Medicine_Main_Category MM
        ON 
            MS.medicine_main_category_id = MM.medicine_main_category_id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medicine_Sub_Category MedicineMainCategory = new Medicine_Sub_Category
                            {
                                medicine_sub_category_id = reader.GetInt32(0),
                                medicine_main_category_id = reader.GetInt32(1),
                                medicine_sub_category_name = reader.GetString(2),
                                Medicine_Main_Category = new Medicine_Main_Category
                                {
                                    medicine_main_category_id = reader.GetInt32(1),
                                    medicine_main_category_name = reader.GetString(3)
                                }
                            };

                            Medicine_Sub_Category.Add(MedicineMainCategory);
                        }
                    }
                    connection.Close();
                }
            }


            return Page();
        }


        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(MedicineSubCategory.medicine_sub_category_name))
            {
                ModelState.AddModelError(nameof(MedicineSubCategory.medicine_sub_category_name), "medicine sub category name cannot be empty.");
                TempData["ErrorMessage"] = "medicine sub category name cannot be empty.";
                return RedirectToPage();
            }

            if (MedicineSubCategory.medicine_main_category_id == 0)
            {
                ModelState.AddModelError(nameof(MedicineSubCategory.medicine_main_category_id), "medicine main category name cannot be empty.");
                TempData["ErrorMessage"] = "medicine main category name cannot be empty.";
                return RedirectToPage();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Medicine_Sub_Category (medicine_main_category_id, medicine_sub_category_name) VALUES (@medicine_main_category_id, @medicine_sub_category_name)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_main_category_id", MedicineSubCategory.medicine_main_category_id);
                    command.Parameters.AddWithValue("@medicine_sub_category_name", MedicineSubCategory.medicine_sub_category_name);
                    command.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Medicine main category added successfully.";
            return RedirectToPage("/Admin/Medicine_list_management/Medicine_Sub_Category_manage/Medicine_Sub_Category");
        }


        public IActionResult OnPostDelete(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid medicine sub category id.");
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Medicine_Sub_Category WHERE medicine_sub_category_id = @medicine_sub_category_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_sub_category_id", id);
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Medicine sub category deleted successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error deleting doctor type.");
                    }
                }
            }

            return RedirectToPage("/Admin/Medicine_list_management/Medicine_Sub_Category_manage/Medicine_Sub_Category");
        }


    }
}
