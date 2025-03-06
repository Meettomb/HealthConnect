using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.UserData
{
    public class Pharmacist_inquiryModel : PageModel
    {
        public List<Pharmacist_approvel> pharmacist_approvels = new List<Pharmacist_approvel>();

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Pharmacist_inquiryModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public string? ProfilePic { get; set; }

        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
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
                                FirstName = reader["first_name"].ToString();
                                LastName = reader["last_name"].ToString();
                                ProfilePic = reader["profile_pic"].ToString();
                                Role = reader["role"].ToString();
                                UserId = reader["id"] as int?;
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
            using SqlConnection connection = new SqlConnection(_connectionString);
            {
                string query = "SELECT * FROM Pharmacist_approvel WHERE account_approve IS NULL";
                using SqlCommand command = new SqlCommand(query, connection);
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            Pharmacist_approvel pharmacist = new Pharmacist_approvel();
                            pharmacist.pharmacist_approvel_id = reader.GetInt32(0);
                            pharmacist.first_name = reader.GetString(1);
                            pharmacist.last_name = reader.GetString(2);
                            pharmacist.email = reader.GetString(3);
                            pharmacist.shop_licence = reader.GetString(15);
                            pharmacist.shop_name = reader.GetString(16);
                            pharmacist.shop_address = reader.GetString(17);
                            pharmacist.account_create_date = reader.GetDateTime(19);
                            pharmacist_approvels.Add(pharmacist);
                        }
                        connection.Close();
                    }
                }
            }
            return Page();
        }
    }
}
