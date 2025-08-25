using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

namespace HealthConnect.Pages.Admin.UserData.UserList
{
    public class Edit_userModel : PageModel
    {
        [BindProperty]
        public User_Table User_Table { get; set; }

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Edit_userModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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

        public IActionResult OnGet(int id)
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
            User_Table userTable = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table WHERE id=@id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userTable = new User_Table
                            {
                                id = reader.GetInt32(0),
                                role = reader.GetString(12)

                            };

                        }
                    }
                }
            }

            if (userTable == null)
            {
                return NotFound(); // Handle no record found
            }

            User_Table = userTable;
            return Page();
        }


        private async Task<string> GetUserEmailAsync(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string fetchEmailQuery = "SELECT email FROM User_Table WHERE id = @id";
                using (SqlCommand fetchCommand = new SqlCommand(fetchEmailQuery, connection))
                {
                    fetchCommand.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var result = await fetchCommand.ExecuteScalarAsync();
                    return result?.ToString();
                }
            }
        }

        public async Task<IActionResult> OnPostRoleUpdate(int id, string role)
        {
            string userEmail = await GetUserEmailAsync(id);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE User_Table SET role = @role WHERE id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@role", role);
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subject = "Account Role Change";
                string body = "Your account role has been change to '{role}' on HealthConnect.";
                string combinedBody = $"{body}\n\nIf you believe this was a mistake or If you have any query, please contact our support team.\n\nRegards,\nHealthConnect Team";

                await _emailService.SendEmailAsync(userEmail, subject, combinedBody);
            }

            return RedirectToPage("/Admin/UserData/UserList/Active_user_data");
        }



    }
}
