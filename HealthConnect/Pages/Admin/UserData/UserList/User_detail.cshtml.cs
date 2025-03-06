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
                                first_name = !reader.IsDBNull(1) ? reader.GetString(1) : null,
                                last_name = !reader.IsDBNull(2) ? reader.GetString(2) : null,
                                email = !reader.IsDBNull(3) ? reader.GetString(3) : null,
                                mobil_no = !reader.IsDBNull(4) ? reader.GetString(4) : null,
                                dob = !reader.IsDBNull(5) ? reader.GetString(5) : null,
                                House_number_and_Street_name = !reader.IsDBNull(6) ? reader.GetString(6) : null,
                                country = !reader.IsDBNull(7) ? reader.GetString(7) : null,
                                city = !reader.IsDBNull(8) ? reader.GetString(8) : null,
                                state = !reader.IsDBNull(9) ? reader.GetString(9) : null,
                                pincode = !reader.IsDBNull(10) ? reader.GetString(10) : null,
                                gender = !reader.IsDBNull(11) ? reader.GetString(11) : null,
                                role = !reader.IsDBNull(12) ? reader.GetString(12) : null,
                                password = !reader.IsDBNull(13) ? reader.GetString(13) : null,
                                profile_pic = !reader.IsDBNull(14) ? reader.GetString(14) : null,
                                doctore_medical_license_photo = !reader.IsDBNull(15) ? reader.GetString(15) : null,
                                medical_registration_no = !reader.IsDBNull(16) ? reader.GetString(16) : null,
                                state_medical_council = !reader.IsDBNull(17) ? reader.GetString(17) : null,
                                year_of_registration = !reader.IsDBNull(18) ? reader.GetString(18) : null,
                                doctore_experience = !reader.IsDBNull(19) ? reader.GetString(19) : null,
                                hospital_or_clinic = !reader.IsDBNull(20) ? reader.GetString(20) : null,
                                doctor_qualifications = !reader.IsDBNull(21) ? reader.GetString(21) : null,
                                doctor_type = !reader.IsDBNull(22) ? reader.GetInt32(22).ToString() : null,

                                //doctor_type = !reader.IsDBNull(22) ? int.Parse(reader.GetString(22)) : (int?)null,




                                languages_spoken = !reader.IsDBNull(23) ? reader.GetString(23) : null,
                                clinic_or_hospital_address = !reader.IsDBNull(24) ? reader.GetString(24) : null,
                                on_site_consultation_fee = !reader.IsDBNull(25) ? reader.GetString(25) : null,
                                account_approve = !reader.IsDBNull(26) ? reader.GetBoolean(26) : false,
                                account_create_date = !reader.IsDBNull(27) ? reader.GetDateTime(27) : DateTime.Now,
                                block = !reader.IsDBNull(28) && reader.GetBoolean(28),
                                isactive = !reader.IsDBNull(29) && reader.GetBoolean(29),
                                mobail_verifie = !reader.IsDBNull(30) && reader.GetBoolean(30),
                                medicine_type = !reader.IsDBNull(32) ? reader.GetString(32) : null,
                                currency_code = !reader.IsDBNull(33) ? reader.GetString(33) : null,
                                video_call_consultation_fee = !reader.IsDBNull(34) ? reader.GetString(34) : null,
                                doctor_specialitis = !reader.IsDBNull(35) ? reader.GetString(35) : null,
                                doctor_profile_complete = !reader.IsDBNull(36) && reader.GetBoolean(36),
                                work_start_time = !reader.IsDBNull(37) ? reader.GetString(37) : null,
                                work_end_time = !reader.IsDBNull(38) ? reader.GetString(38) : null,
                                weekly_work_days = !reader.IsDBNull(39) ? reader.GetString(39).Split(',').ToList() : new List<string>(),
                                max_time_per_appointments = !reader.IsDBNull(40) ? reader.GetString(40) : null,
                                break_between_two_appointments = !reader.IsDBNull(41) ? reader.GetString(41) : null
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