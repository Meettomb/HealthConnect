using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HealthConnect.Pages.User
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly string _connectionString;

        public LogoutModel(ILogger<LogoutModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the current device's auth_token from the cookie
            string deviceId = Request.Cookies["deviceUniqueId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                // If the device ID is not found in cookies, no need to log out
                return RedirectToPage("/Index");
            }

            int? userId = HttpContext.Session.GetInt32("Id");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT id, auth_token
                         FROM User_Table
                         WHERE id = @UserId";

                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string authTokens = reader["auth_token"]?.ToString() ?? string.Empty;

                        // If the auth_token exists, remove the current device's token from the list
                        if (!string.IsNullOrEmpty(authTokens) && authTokens.Contains(deviceId))
                        {
                            var tokensList = authTokens.Split(',').Where(token => token != deviceId).ToList();
                            string updatedTokens = string.Join(",", tokensList);

                            // Now that the reader is closed, update the database with the new list of auth_tokens
                            reader.Close();  // Close the reader before executing another query.

                            string updateQuery = "UPDATE User_Table SET auth_token = @AuthToken WHERE id = @UserId";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@AuthToken", updatedTokens);
                                updateCmd.Parameters.AddWithValue("@UserId", userId.Value);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    reader.Close();  // Make sure to close the reader after reading data
                }

                // Remove the device-specific auth_token from the cookie
                Response.Cookies.Delete("deviceUniqueId");

                // Clear session data
                HttpContext.Session.Remove("Id");
            }

            return RedirectToPage("/Index");
        }
    }
}
