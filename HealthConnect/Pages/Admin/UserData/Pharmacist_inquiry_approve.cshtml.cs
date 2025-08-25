using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.UserData
{
    public class Pharmacist_inquiry_approveModel : PageModel
    {
        [BindProperty]
        public Pharmacist_approvel pharmacist_Approvel { get; set; }

        public string TypeOfDoctor { get; set; }


        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string ProfilePic { get; set; }

        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public Pharmacist_inquiry_approveModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        public int? UserId { get; set; }

        public IActionResult OnGet(int pharmacist_approvel_id)
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
            Pharmacist_approvel pharmacist = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string PharmacistQuery = "SELECT * FROM Pharmacist_approvel WHERE pharmacist_approvel_id=@pharmacist_approvel_id";


                using (SqlCommand pharmacistCommand = new SqlCommand(PharmacistQuery, connection))
                {
                    pharmacistCommand.Parameters.AddWithValue("@pharmacist_approvel_id", pharmacist_approvel_id);
                    connection.Open();

                    using (SqlDataReader pharmacistReader = pharmacistCommand.ExecuteReader())
                    {
                        if (pharmacistReader.Read())
                        {
                            pharmacist = new Pharmacist_approvel
                            {
                                pharmacist_approvel_id = pharmacistReader.GetInt32(0),
                                first_name = pharmacistReader.GetString(1),
                                last_name = pharmacistReader.GetString(2),
                                email = pharmacistReader.GetString(3),
                                mobil_no = pharmacistReader.GetString(4),
                                dob = pharmacistReader.GetString(5),
                                House_number_and_Street_name = pharmacistReader.GetString(6),
                                country = pharmacistReader.GetString(7),
                                city = pharmacistReader.GetString(8),
                                state = pharmacistReader.GetString(9),
                                pincode = pharmacistReader.GetString(10),
                                gender = pharmacistReader.GetString(11),
                                role = pharmacistReader.GetString(12),
                                password = pharmacistReader.GetString(13),
                                currency_code = pharmacistReader.GetString(14),

                                shop_licence = pharmacistReader.GetString(15),
                                shop_name = pharmacistReader.GetString(16),
                                shop_address = pharmacistReader.GetString(17),

                                isactive = pharmacistReader.GetBoolean(18),

                                account_create_date = pharmacistReader.GetDateTime(19),
                                account_approve = !pharmacistReader.IsDBNull(20) ? pharmacistReader.GetBoolean(20) : (bool?)null,
                               
                               
                            };
                        }
                    }

                }
            }


            if (pharmacist == null)
            {
                return NotFound();
            }

            pharmacist_Approvel = pharmacist;
            return Page();
        }


        public IActionResult OnPostApprove(int pharmacist_approvel_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Pharmacist_approvel WHERE pharmacist_approvel_id = @Id", connection);
                command.Parameters.AddWithValue("@Id", pharmacist_approvel_id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var pharmacist = new User_Table
                    {
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
                        currency_code = reader.IsDBNull(14) ? null : reader.GetString(14),

                        account_create_date = DateTime.Now,
                        account_approve = true,
                        isactive = true,
                        //block = false,
                        //mobail_verifie = false,

                        shop_licence = reader.GetString(15),
                        shop_name = reader.GetString(16),
                        shop_address = reader.GetString(17),

                    };
                    reader.Close();

                    var insertUserCommand = new SqlCommand("INSERT INTO User_Table (first_name, last_name, email, mobil_no, dob, House_number_and_Street_name, country, city, state, pincode, gender, role, password, currency_code, shop_licence, shop_name, shop_address, account_approve, account_create_date, isactive, block, mobail_verifie) VALUES (@FirstName, @LastName, @Email, @MobileNo, @Dob, @Address, @Country, @City, @State, @Pincode, @Gender, @Role, @Password, @Currency_code, @ShopLicence, @ShopName, @ShopAddress, @Approve, @CreateDate, @IsActive, @Block, @MobailVerifie)", connection);

                    insertUserCommand.Parameters.AddWithValue("@FirstName", pharmacist.first_name);
                    insertUserCommand.Parameters.AddWithValue("@LastName", pharmacist.last_name);
                    insertUserCommand.Parameters.AddWithValue("@Email", pharmacist.email);
                    insertUserCommand.Parameters.AddWithValue("@MobileNo", pharmacist.mobil_no);
                    insertUserCommand.Parameters.AddWithValue("@Dob", pharmacist.dob);
                    insertUserCommand.Parameters.AddWithValue("@Address", pharmacist.House_number_and_Street_name);
                    insertUserCommand.Parameters.AddWithValue("@Country", pharmacist.country);
                    insertUserCommand.Parameters.AddWithValue("@City", pharmacist.city);
                    insertUserCommand.Parameters.AddWithValue("@State", pharmacist.state);
                    insertUserCommand.Parameters.AddWithValue("@Pincode", pharmacist.pincode);
                    insertUserCommand.Parameters.AddWithValue("@Gender", pharmacist.gender);
                    insertUserCommand.Parameters.AddWithValue("@Role", pharmacist.role);
                    insertUserCommand.Parameters.AddWithValue("@Password", pharmacist.password);
                    insertUserCommand.Parameters.AddWithValue("@Currency_code", pharmacist.currency_code);
                    insertUserCommand.Parameters.AddWithValue("@ShopLicence", pharmacist.shop_licence);
                    insertUserCommand.Parameters.AddWithValue("@ShopName", pharmacist.shop_name);
                    insertUserCommand.Parameters.AddWithValue("@ShopAddress", pharmacist.shop_address);
                    insertUserCommand.Parameters.AddWithValue("@Approve", pharmacist.account_approve);
                    insertUserCommand.Parameters.AddWithValue("@CreateDate", pharmacist.account_create_date);
                    insertUserCommand.Parameters.AddWithValue("@IsActive", pharmacist.isactive);
                    insertUserCommand.Parameters.AddWithValue("@Block", false);
                    insertUserCommand.Parameters.AddWithValue("@MobailVerifie", false);

                    insertUserCommand.ExecuteNonQuery();


                    var updateCommand = new SqlCommand("UPDATE Pharmacist_approvel SET isactive = @IsActive, account_approve = @Approve WHERE pharmacist_approvel_id = @Id", connection);
                    updateCommand.Parameters.AddWithValue("@IsActive", true);
                    updateCommand.Parameters.AddWithValue("@Approve", true);
                    updateCommand.Parameters.AddWithValue("@Id", pharmacist_approvel_id);
                    updateCommand.ExecuteNonQuery();

                    SendApprovalEmail(pharmacist.email);
                }
            }
            return RedirectToPage("/Admin/UserData/Pharmacist_inquiry");
        }

        public IActionResult OnPostReject(int pharmacist_approvel_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Pharmacist_approvel SET account_approve = @Approve WHERE pharmacist_approvel_id = @Id", connection);

                command.Parameters.AddWithValue("@Approve", false);
                command.Parameters.AddWithValue("@Id", pharmacist_approvel_id);
                command.ExecuteNonQuery();

                string email = GetDoctorEmail(pharmacist_approvel_id);
                SendRejectionEmail(email);
            }
            return RedirectToPage("/Admin/UserData/Pharmacist_inquiry");
        }

        private string GetDoctorEmail(int pharmacist_approvel_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT email FROM Pharmacist_approvel WHERE pharmacist_approvel_id = @Id", connection);
                command.Parameters.AddWithValue("@Id", pharmacist_approvel_id);

                var result = command.ExecuteScalar();
                return result?.ToString();
            }
        }

        private async Task SendRejectionEmail(string email)
        {
            string emailBody = "Your account will be rejected. Thank you";

            var emailSent = await _emailService.SendEmailAsync(
                email,
                "Pharmacist Registration Verification",
                emailBody
            );
        }

        private async Task SendApprovalEmail(string email)
        {
            string emailBody = "Your account will be activated. Thank you";

            var emailSent = await _emailService.SendEmailAsync(
                email,
                "Pharmacist Registration Verification",
                emailBody
            );
        }


    }
}
