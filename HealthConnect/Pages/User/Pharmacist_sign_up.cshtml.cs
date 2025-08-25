using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.User
{
    public class Pharmacist_sign_upModel : PageModel
    {

        private readonly ILogger<Doctor_sign_upModel> _logger;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;
        public Pharmacist_sign_upModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");

        }


        [BindProperty]
        public Pharmacist_approvel pharmacist_approvel { get; set; }

        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Gender { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string Account_create_date { get; set; }
        public string CurrencyCode { get; set; }

        public int? UserId { get; set; }

        public string SessionOtp { get; set; }

        public IActionResult OnGet()
        {
            FirstName = HttpContext.Session.GetString("FirstName");
            LastName = HttpContext.Session.GetString("LastName");
            Email = HttpContext.Session.GetString("Email");
            MobileNo = HttpContext.Session.GetString("MobileNo");
            DOB = HttpContext.Session.GetString("DOB");
            Address = HttpContext.Session.GetString("Address");
            Country = HttpContext.Session.GetString("Country");
            City = HttpContext.Session.GetString("City");
            State = HttpContext.Session.GetString("State");
            Pincode = HttpContext.Session.GetString("Pincode");
            Gender = HttpContext.Session.GetString("Gender");
            Role = HttpContext.Session.GetString("Role");
            Password = HttpContext.Session.GetString("Password");
            Account_create_date = HttpContext.Session.GetString("Account_create_date");
            CurrencyCode = HttpContext.Session.GetString("CurrencyCode");

            SessionOtp = HttpContext.Session.GetString("OTP");

            UserId = HttpContext.Session.GetInt32("Id");
            string roleInSession = HttpContext.Session.GetString("UserRole");


            if (UserId.HasValue && !string.IsNullOrEmpty(roleInSession))
            {
                if (roleInSession == "Admin")
                {
                    return RedirectToPage("/Admin/Admin_index");
                }
                else if (roleInSession == "User" || roleInSession == "Doctor" || roleInSession == "Pharmacist")
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
                            else if (role == "User" || role == "Doctor" || role == "Pharmacist")
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
            if (Request.Form.Files.Count == 0)
            {
                ErrorMessage = "Please upload license photo.";
                return Page();
            }

            var licensePic = Request.Form.Files["shop_licence"];

            if (licensePic == null)
            {
                ErrorMessage = "license photo are required.";
                return Page();
            }

            var licensePicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documant", licensePic.FileName);


            using (var stream = new FileStream(licensePicPath, FileMode.Create))
            {
                await licensePic.CopyToAsync(stream);
            }

            pharmacist_approvel.shop_licence = licensePic.FileName;

            pharmacist_approvel.first_name = HttpContext.Session.GetString("FirstName");
            pharmacist_approvel.last_name = HttpContext.Session.GetString("LastName");
            pharmacist_approvel.email = HttpContext.Session.GetString("Email");
            pharmacist_approvel.mobil_no = HttpContext.Session.GetString("MobileNo");
            pharmacist_approvel.dob = HttpContext.Session.GetString("DOB");
            pharmacist_approvel.House_number_and_Street_name = HttpContext.Session.GetString("Address");
            pharmacist_approvel.country = HttpContext.Session.GetString("Country");
            pharmacist_approvel.city = HttpContext.Session.GetString("City");
            pharmacist_approvel.state = HttpContext.Session.GetString("State");
            pharmacist_approvel.pincode = HttpContext.Session.GetString("Pincode");
            pharmacist_approvel.gender = HttpContext.Session.GetString("Gender");
            pharmacist_approvel.role = HttpContext.Session.GetString("Role");
            pharmacist_approvel.currency_code = HttpContext.Session.GetString("CurrencyCode");
            var hashedPassword = new PasswordHasher<Pharmacist_approvel>().HashPassword(pharmacist_approvel, HttpContext.Session.GetString("Password"));
            pharmacist_approvel.password = hashedPassword;

            pharmacist_approvel.account_create_date = DateTime.Now;
            pharmacist_approvel.shop_name = Request.Form["shop_name"];
            pharmacist_approvel.shop_address = Request.Form["shop_address"];

            bool isSaved = await SavePharmacistApprovelToDatabase(pharmacist_approvel);
            if (!isSaved)
            {
                ErrorMessage = "An error occurred while saving your information.";
                return Page();
            }

            string emailBody = "Your account will be activated after verifying your details. This process may take a few days.";
            var emailSent = await _emailService.SendEmailAsync(
                pharmacist_approvel.email,
                "Pharmacist Registration Verification",
                emailBody
            );

            if (emailSent)
            {
                TempData["SuccessMessage"] = "Your registration is successful. Please check your email for verification.";
                SuccessMessage = "Your registration is successful. Please check your email for verification.";
                HttpContext.Session.Clear();
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while sending the email.";
                ErrorMessage = "An error occurred while sending the email.";
            }

            return RedirectToPage("/Index");
        }

        private async Task<bool> SavePharmacistApprovelToDatabase(Pharmacist_approvel pharmacist_Approvel)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO Pharmacist_approvel 
                    (first_name, last_name, email, mobil_no, dob, House_number_and_Street_name, country, city, state, pincode, gender, role, password, 
                     account_create_date, shop_licence, shop_name, shop_address, isactive, currency_code)
                  VALUES 
                    (@FirstName, @LastName, @Email, @MobilNo, @DOB, @HouseNoStreetName, @Country, @City, @State, @Pincode, @Gender, @Role, @Password, 
                    @AccountCreateDate, @ShopLicence, @ShopName, @ShopAddress, @IsActive, @Currency_code)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", pharmacist_Approvel.first_name);
                    command.Parameters.AddWithValue("@LastName", pharmacist_Approvel.last_name);
                    command.Parameters.AddWithValue("@Email", pharmacist_Approvel.email);
                    command.Parameters.AddWithValue("@MobilNo", pharmacist_Approvel.mobil_no);
                    command.Parameters.AddWithValue("@DOB", pharmacist_Approvel.dob);
                    command.Parameters.AddWithValue("@HouseNoStreetName", pharmacist_Approvel.House_number_and_Street_name);
                    command.Parameters.AddWithValue("@Country", pharmacist_Approvel.country);
                    command.Parameters.AddWithValue("@City", pharmacist_Approvel.city);
                    command.Parameters.AddWithValue("@State", pharmacist_Approvel.state);
                    command.Parameters.AddWithValue("@Pincode", pharmacist_Approvel.pincode);
                    command.Parameters.AddWithValue("@Gender", pharmacist_Approvel.gender);
                    command.Parameters.AddWithValue("@Role", pharmacist_Approvel.role);
                    command.Parameters.AddWithValue("@Password", pharmacist_Approvel.password);

                    command.Parameters.AddWithValue("@AccountCreateDate", pharmacist_Approvel.account_create_date);
                    command.Parameters.AddWithValue("@ShopLicence", pharmacist_approvel.shop_licence);
                    command.Parameters.AddWithValue("@ShopName", pharmacist_approvel.shop_name);
                    command.Parameters.AddWithValue("@ShopAddress", pharmacist_approvel.shop_address);
                    command.Parameters.AddWithValue("@IsActive", false);
                    command.Parameters.AddWithValue("@Currency_code", pharmacist_Approvel.currency_code);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


    }
}
