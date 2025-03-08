using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Medicine_list_management.Medicine_Finel_Category_manage
{
    public class Medicine_Finel_CategoryModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Medicine_Main_Category MedicineMainCategory { get; set; } = new Medicine_Main_Category();

        public List<Medicine_Main_Category> Medicine_Main_Category { get; set; } = new List<Medicine_Main_Category>();


        public List<Medicine_Main_Category> Medicine_Main_CategoryList = new List<Medicine_Main_Category>();

        [BindProperty]
        public Medicine_Sub_Category MedicineSubCategory { get; set; } = new Medicine_Sub_Category();

        public List<Medicine_Sub_Category> Medicine_Sub_Category { get; set; } = new List<Medicine_Sub_Category>();

        [BindProperty]
        public Medicine_Finel_Category MedicineFinelCategory { get; set; } = new Medicine_Finel_Category();


        public List<Medicine_Finel_Category> Medicine_Finel_Category { get; set; } = new List<Medicine_Finel_Category>();



        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public Medicine_Finel_CategoryModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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
                string query = "SELECT * FROM Medicine_Sub_Category";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            Medicine_Sub_Category MedicineSubCategory = new Medicine_Sub_Category();
                            MedicineSubCategory.medicine_sub_category_id = reader.GetInt32(0);
                            MedicineSubCategory.medicine_sub_category_name = reader.GetString(2);

                            Medicine_Sub_Category.Add(MedicineSubCategory);
                        }
                    }
                    connection.Close();
                }
            }





            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
    SELECT 
        MF.medicine_finel_category_id,
        MF.medicine_main_category_id,
        MF.medicine_sub_category_id,
        MF.medicine_finel_category_name,
        MM.medicine_main_category_name,
        MS.medicine_sub_category_name
    FROM 
        Medicine_Finel_Category MF
    JOIN 
        Medicine_Main_Category MM 
    ON 
        MF.medicine_main_category_id = MM.medicine_main_category_id
    JOIN 
        Medicine_Sub_Category MS 
    ON 
        MF.medicine_sub_category_id = MS.medicine_sub_category_id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medicine_Finel_Category finelCategory = new Medicine_Finel_Category
                            {
                                medicine_finel_category_id = reader.GetInt32(0),
                                medicine_main_category_id = reader.GetInt32(1),
                                medicine_sub_category_id = reader.GetInt32(2),
                                medicine_finel_category_name = reader.GetString(3),
                                Medicine_Main_Category = new Medicine_Main_Category
                                {
                                    medicine_main_category_id = reader.GetInt32(1),
                                    medicine_main_category_name = reader.GetString(4)
                                },
                                Medicine_Sub_Category = new Medicine_Sub_Category
                                {
                                    medicine_sub_category_id = reader.GetInt32(2),
                                    medicine_sub_category_name = reader.GetString(5)
                                }
                            };

                            Medicine_Finel_Category.Add(finelCategory);
                        }
                    }
                }
            }


            return Page();
        }


        public IActionResult OnPost()
        {
            if (MedicineFinelCategory.medicine_main_category_id == 0)
            {
                TempData["ErrorMessage"] = "Medicine main category cannot be empty.";
                return RedirectToPage();
            }

            if (MedicineFinelCategory.medicine_sub_category_id == 0)
            {
                TempData["ErrorMessage"] = "Medicine sub category cannot be empty.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(MedicineFinelCategory.medicine_finel_category_name))
            {
                TempData["ErrorMessage"] = "Medicine final category name cannot be empty.";
                return RedirectToPage();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Medicine_Finel_Category (medicine_main_category_id, medicine_sub_category_id, medicine_finel_category_name) " +
                               "VALUES (@medicine_main_category_id, @medicine_sub_category_id, @medicine_finel_category_name)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_main_category_id", MedicineFinelCategory.medicine_main_category_id);
                    command.Parameters.AddWithValue("@medicine_sub_category_id", MedicineFinelCategory.medicine_sub_category_id);
                    command.Parameters.AddWithValue("@medicine_finel_category_name", MedicineFinelCategory.medicine_finel_category_name);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Medicine final category added successfully.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to insert data.";
                    }
                }
            }

            return RedirectToPage("/Admin/Medicine_list_management/Medicine_Finel_Category_manage/Medicine_Finel_Category");
        }


        public IActionResult OnPostDelete(int id)
        {
            if (id <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid medicine finel category id.");
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Medicine_Finel_Category WHERE medicine_finel_category_id = @medicine_finel_category_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@medicine_finel_category_id", id);
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Medicine finel category deleted successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error deleting doctor type.");
                    }
                }
            }

            return RedirectToPage("/Admin/Medicine_list_management/Medicine_Finel_Category_manage/Medicine_Finel_Category");
        }



    }
}
