using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace HealthConnect.Pages.Admin.Pharmaceutical_brands
{
    public class Pharmaceutical_brands_editModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Pharmaceutical_Brands PharmaceuticalBrands { get; set; } = new Pharmaceutical_Brands();

        public List<Pharmaceutical_Brands> pharmaceutical_brands { get; set; } = new List<Pharmaceutical_Brands>();


        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }
        [BindProperty]
        public string? ErrorMessage { get; set; }


        public Pharmaceutical_brands_editModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int pharmaceutical_brands_id)
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
                string query = "SELECT * FROM Pharmaceutical_Brands WHERE pharmaceutical_brands_id = @pharmaceutical_brands_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@pharmaceutical_brands_id", SqlDbType.Int).Value = pharmaceutical_brands_id;
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            PharmaceuticalBrands = new Pharmaceutical_Brands
                            {
                                pharmaceutical_brands_id = reader.GetInt32(0),
                                pharmaceutical_brands_image = reader.GetString(1),
                                pharmaceutical_brands_name = reader.GetString(2)
                            };
                        }
                    }
                }
            }


            return Page();
        }


        public async Task<IActionResult> OnPostAsync(int pharmaceutical_brands_id)
        {
            if (Request.Form.Files.Count == 0 && string.IsNullOrWhiteSpace(Request.Form["pharmaceutical_brands_name"]))
            {
                ErrorMessage = "Please upload a pharmaceutical brand image or provide a name.";
                return Page();
            }

            var pharmaceuticalBrandsImage = Request.Form.Files["pharmaceutical_brands_image"];
            var pharmaceuticalBrandsName = Request.Form["pharmaceutical_brands_name"].ToString();

            if (string.IsNullOrWhiteSpace(pharmaceuticalBrandsName))
            {
                ErrorMessage = "Pharmaceutical brand name is required.";
                return Page();
            }

            string newImage = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT pharmaceutical_brands_image FROM Pharmaceutical_Brands WHERE pharmaceutical_brands_id = @pharmaceutical_brands_id";
                string oldImage = null;

                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@pharmaceutical_brands_id", pharmaceutical_brands_id);
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldImage = reader["pharmaceutical_brands_image"]?.ToString();
                        }
                    }
                }

                if (pharmaceuticalBrandsImage != null && pharmaceuticalBrandsImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PharmaceuticalBrandsImage");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string fileName = pharmaceuticalBrandsImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await pharmaceuticalBrandsImage.CopyToAsync(fileStream);
                    }

                    newImage = fileName;
                }
                else
                {
                    newImage = oldImage;
                }

                string updateQuery = "UPDATE Pharmaceutical_Brands SET pharmaceutical_brands_image = @pharmaceutical_brands_image, pharmaceutical_brands_name = @pharmaceutical_brands_name WHERE pharmaceutical_brands_id = @pharmaceutical_brands_id";

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@pharmaceutical_brands_image", newImage ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@pharmaceutical_brands_name", string.IsNullOrWhiteSpace(pharmaceuticalBrandsName) ? (object)DBNull.Value : pharmaceuticalBrandsName);
                    command.Parameters.AddWithValue("@pharmaceutical_brands_id", pharmaceutical_brands_id);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Pharmaceutical brand updated successfully!";
                        return RedirectToPage("/Admin/Pharmaceutical_brands/Pharmaceutical_brands");
                    }
                    else
                    {
                        ErrorMessage = "Something went wrong. Please try again.";
                    }
                }
            }

            return Page();
        }





    }
}
