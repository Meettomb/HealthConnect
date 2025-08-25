using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;
using HealthConnect.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

namespace HealthConnect.Pages.User
{
    public class Sign_inModel : PageModel
    {
        [BindProperty]
        public User_Table User { get; set; }

        private readonly IConfiguration _configuration;
        private static Random random = new Random();
        private readonly string _connectionString;

        public Sign_inModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HealthConnect");
            _configuration = configuration;
        }

        public int? UserId { get; set; }
        public string StoredAuthToken { get; set; }
        public string DeviceId { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            string roleInSession = HttpContext.Session.GetString("UserRole");

            if (UserId.HasValue && !string.IsNullOrEmpty(roleInSession))
            {
                if (roleInSession == "Admin")
                {
                    return RedirectToPage("/Admin/Admin_index");
                }
                else if (roleInSession == "User" || roleInSession == "Doctor")
                {
                    return RedirectToPage("/index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid role in session.");
                }
            }

            string deviceId = Request.Cookies["deviceUniqueId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT id, first_name, last_name, email, role, isactive 
                         FROM User_Table 
                         WHERE ',' + auth_token + ',' LIKE '%,' + @DeviceId + ',%'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["id"]);
                        string firstName = reader["first_name"].ToString();
                        string lastName = reader["last_name"].ToString();
                        string email = reader["email"].ToString();
                        string role = reader["role"].ToString();
                        bool isActive = Convert.ToBoolean(reader["isactive"]);

                        if (isActive)
                        {
                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("FirstName", firstName);
                            HttpContext.Session.SetString("LastName", lastName);
                            HttpContext.Session.SetString("Email", email);
                            HttpContext.Session.SetString("UserRole", role);

                            // Redirect based on role
                            if (role == "Admin")
                            {
                                return RedirectToPage("/Admin/Admin_index");
                            }
                            else if (role == "User" || role == "Doctor")
                            {
                                return RedirectToPage("/index");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid role.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Account is not active.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Device not recognized or account not found.");
                    }
                }
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(User.email) || string.IsNullOrEmpty(User.password))
            {
                ErrorMessage = "Email and Password are required.";
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT id, password, auth_token, role, isactive, block
                         FROM User_Table 
                         WHERE email = @Email AND isactive = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", User.email);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string storedPassword = reader["password"].ToString();
                        int userId = Convert.ToInt32(reader["id"]);
                        string role = reader["role"].ToString();
                        bool isActive = Convert.ToBoolean(reader["isactive"]);
                        bool isBlocked = Convert.ToBoolean(reader["block"]);
                        string auth_token = reader["auth_token"]?.ToString() ?? string.Empty;

                        reader.Close();

                        if (isBlocked)
                        {
                            ModelState.AddModelError(string.Empty, "Your account is blocked. Please contact support.");
                            ErrorMessage = "Your account is blocked. Please contact support.";
                            return Page();
                        }

                        var passwordHasher = new PasswordHasher<object>();
                        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, storedPassword, User.password);

                        if (passwordVerificationResult == PasswordVerificationResult.Success)
                        {
                            if (!isActive)
                            {
                                ModelState.AddModelError(string.Empty, "Account is not active.");
                                ErrorMessage = "Account is not active.";
                                return Page();
                            }

                            string deviceId = Request.Cookies["deviceUniqueId"];
                            if (string.IsNullOrEmpty(deviceId))
                            {
                                deviceId = "id-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Guid.NewGuid().ToString("N");
                                Response.Cookies.Append("deviceUniqueId", deviceId, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
                            }

                            string newAuthTokens = string.IsNullOrEmpty(auth_token) ? deviceId : auth_token + "," + deviceId;

                            string updateAuthTokenQuery = "UPDATE User_Table SET auth_token = @AuthToken WHERE id = @UserId";
                            using (SqlCommand updateCmd = new SqlCommand(updateAuthTokenQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@AuthToken", newAuthTokens);
                                updateCmd.Parameters.AddWithValue("@UserId", userId);
                                updateCmd.ExecuteNonQuery();
                            }

                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("UserRole", role);

                            if (role == "Admin")
                            {
                                return RedirectToPage("/Admin/Admin_index");
                            }
                            else if (role == "User" || role == "Doctor")
                            {
                                return RedirectToPage("/index");
                            }
                            else if (role == "Pharmacist")
                            {
                                return RedirectToPage("/Pharmacy");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid role.");
                                ErrorMessage = "Invalid role.";
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                            ErrorMessage = "Invalid password.";
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email not found.");
                        ErrorMessage = "Email not found.";
                    }

                    con.Close();
                }
            }
            return Page();
        }




    }
}
