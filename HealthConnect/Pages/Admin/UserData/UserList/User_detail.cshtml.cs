using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.UserData.UserList
{
    public class User_detailModel : PageModel
    {
        [BindProperty]
        public User_Table User_Table { get; set; }

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public User_detailModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
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
                                first_name = reader.GetString(1),
                                last_name = reader.GetString(2),
                                email = reader.GetString(3),
                                mobil_no = reader.GetString(4),
                                dob = reader.GetString(5),
                                House_number_and_Street_name = reader.GetString(6),
                                country = reader.GetString(7),
                                city = reader.GetString(8),
                                state = reader.GetString(9),
                                pincode = reader.GetString(10),
                                gender = reader.GetString(11),
                                role = reader.GetString(12),
                                password = reader.GetString(13),
                                profile_pic = !reader.IsDBNull(14) ? reader.GetString(14) : null,
                                doctore_medical_license_photo = !reader.IsDBNull(15) ? reader.GetString(15) : null,
                                medical_registration_no = !reader.IsDBNull(16) ? reader.GetString(16) : null,
                                state_medical_council = !reader.IsDBNull(17) ? reader.GetString(17) : null,
                                year_of_registration = !reader.IsDBNull(18) ? reader.GetString(18) : null,
                                doctore_experience = !reader.IsDBNull(19) ? reader.GetString(19) : null,
                                hospital_or_clinic = !reader.IsDBNull(20) ? reader.GetString(20) : null,
                                doctor_qualifications = !reader.IsDBNull(21) ? reader.GetString(21) : null,
                                doctor_type = !reader.IsDBNull(22) ? reader.GetString(22) : null,
                                languages_spoken = !reader.IsDBNull(23) ? reader.GetString(23) : null,
                                clinic_or_hospital_address = !reader.IsDBNull(24) ? reader.GetString(24) : null,
                                on_site_consultation_fee = !reader.IsDBNull(25) ? reader.GetString(25) : null,
                                account_approve = !reader.IsDBNull(26) ? reader.GetBoolean(26) : false, 
                                account_create_date = !reader.IsDBNull(27) ? reader.GetDateTime(27) : DateTime.Now,
                                block = reader.GetBoolean(28),
                                isactive = reader.GetBoolean(29),
                                medicine_type = !reader.IsDBNull(32) ? reader.GetString(32) : null,
                                currency_code = !reader.IsDBNull(33) ? reader.GetString(33) : null,
                                video_call_consultation_fee = !reader.IsDBNull(34) ? reader.GetString(34) : null,
                            };

                        }
                    }
                }
            }

            if (userTable == null)
            {
                return NotFound();
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


        public async Task<IActionResult> OnPostDelete(int id)
        {
            string userEmail = await GetUserEmailAsync(id);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE User_Table SET isactive = @isactive WHERE id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@isactive", false);
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subject = "Account Deletion";
                string body = "Your account has been deleted on HealthConnect.";
                string combinedBody = $"{body}\n\nIf you believe this was a mistake, please contact our support team.\n\nRegards,\nHealthConnect Team";

                await _emailService.SendEmailAsync(userEmail, subject, combinedBody);
            }

            return RedirectToPage("/Admin/UserData/UserList/Active_user_data");
        }

        public async Task<IActionResult> OnPostActive(int id)
        {
            string userEmail = await GetUserEmailAsync(id);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE User_Table SET isactive = @isactive WHERE id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@isactive", true);
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subject = "Account Active";
                string body = "Your account has been Activated on HealthConnect.";
                string combinedBody = $"{body}\n\nIf you any query, please contact our support team.\n\nRegards,\nHealthConnect Team";

                await _emailService.SendEmailAsync(userEmail, subject, combinedBody);
            }

            return RedirectToPage("/Admin/UserData/UserList/Active_user_data");
        }



        public async Task<IActionResult> OnPostBlock(int id)
        {
            string userEmail = await GetUserEmailAsync(id);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE User_Table SET block = @block WHERE id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@block", true);
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subject = "Account Blocked";
                string body = "Your account has been blocked on HealthConnect.";
                string combinedBody = $"{body}\n\nIf you believe this was a mistake, please contact our support team.\n\nRegards,\nHealthConnect Team";

                await _emailService.SendEmailAsync(userEmail, subject, combinedBody);
            }

            return RedirectToPage("/Admin/UserData/UserList/Active_user_data");
        }

        public async Task<IActionResult> OnPostUnblock(int id)
        {
            string userEmail = await GetUserEmailAsync(id);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE User_Table SET block = @block WHERE id = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@block", false);
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                }
            }

            if (!string.IsNullOrEmpty(userEmail))
            {
                string subject = "Account Unblock";
                string body = "Your account has been Unblock on HealthConnect.";
                string combinedBody = $"{body}\n\nIf you any query, please contact our support team.\n\nRegards,\nHealthConnect Team";

                await _emailService.SendEmailAsync(userEmail, subject, combinedBody);
            }

            return RedirectToPage("/Admin/UserData/UserList/Active_user_data");
        }



    }
}
