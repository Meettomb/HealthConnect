using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.Pharmaceutical_brands
{
    public class Pharmaceutical_brandsModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        [BindProperty]
        public Pharmaceutical_Brands PharmaceuticalBrands { get; set; } = new Pharmaceutical_Brands();

        public List<Pharmaceutical_Brands> Pharmaceutical_Brands { get; set; } = new List<Pharmaceutical_Brands>();



        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }
        [BindProperty]
        public string? ErrorMessage { get; set; }


        public Pharmaceutical_brandsModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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
                string query = "SELECT * FROM Pharmaceutical_Brands";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pharmaceutical_Brands Pharmaceutical_Brands_Name = new Pharmaceutical_Brands();
                            Pharmaceutical_Brands_Name.pharmaceutical_brands_id = reader.GetInt32(0);
                            Pharmaceutical_Brands_Name.pharmaceutical_brands_image = reader.GetString(1);
                            Pharmaceutical_Brands_Name.pharmaceutical_brands_name = reader.GetString(2);

                            Pharmaceutical_Brands.Add(Pharmaceutical_Brands_Name);
                        }
                    }
                    connection.Close();
                }
            }


            return Page();
        }


        public async Task<IActionResult> OnPost()
        {
            if (Request.Form.Files.Count == 0)
            {
                ErrorMessage = "Please upload pharmaceutical brands image.";
                return Page();
            }

            var pharmaceuticalBrandsImage = Request.Form.Files["pharmaceutical_brands_image"];
            var pharmaceuticalBrandsName = Request.Form["pharmaceutical_brands_name"];

            if (pharmaceuticalBrandsImage == null)
            {
                ErrorMessage = "Pharmaceutical brands image is required.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(pharmaceuticalBrandsName))
            {
                ErrorMessage = "Pharmaceutical brands name is required.";
                return Page();
            }

            var profilePicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "PharmaceuticalBrandsImage", pharmaceuticalBrandsImage.FileName);

            try
            {
                using (var stream = new FileStream(profilePicPath, FileMode.Create))
                {
                    await pharmaceuticalBrandsImage.CopyToAsync(stream);
                }

                PharmaceuticalBrands.pharmaceutical_brands_image = pharmaceuticalBrandsImage.FileName;
                PharmaceuticalBrands.pharmaceutical_brands_name = pharmaceuticalBrandsName;

                using SqlConnection con = new SqlConnection(_connectionString);
                {
                    string query = "INSERT INTO Pharmaceutical_Brands (pharmaceutical_brands_image, pharmaceutical_brands_name) VALUES (@pharmaceutical_brands_image, @pharmaceutical_brands_name)";
                    using SqlCommand cmd = new SqlCommand(query, con);
                    {
                        cmd.Parameters.AddWithValue("@pharmaceutical_brands_image", PharmaceuticalBrands.pharmaceutical_brands_image);
                        cmd.Parameters.AddWithValue("@pharmaceutical_brands_name", PharmaceuticalBrands.pharmaceutical_brands_name);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();

                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Pharmaceutical brand added successfully!";
                            return RedirectToPage("/Admin/Pharmaceutical_brands/Pharmaceutical_brands"); 
                        }
                        else
                        {
                            ErrorMessage = "Something went wrong. Please try again.";
                            return Page();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while adding the brand. Please try again.";
                Console.WriteLine(ex.Message);
                return Page();
            }
        }

        public IActionResult OnPostDelete(int pharmaceutical_brands_id)
        {
            if (pharmaceutical_brands_id <= 0)
            {
                ModelState.AddModelError(string.Empty, "Invalid pharmaceutical brands.");
                return Page();
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Pharmaceutical_Brands WHERE pharmaceutical_brands_id = @pharmaceutical_brands_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pharmaceutical_brands_id", pharmaceutical_brands_id);
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Pharmaceutical brand deleted successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error deleting Pharmaceutical brand.");
                    }
                }
            }

            return RedirectToPage("/Admin/Pharmaceutical_brands/Pharmaceutical_brands");
        }


    }
}
