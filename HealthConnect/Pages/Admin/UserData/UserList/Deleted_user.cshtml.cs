using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace HealthConnect.Pages.Admin.UserData.UserList
{
    public class Deleted_userModel : PageModel
    {
        public List<User_Table> User_Table = new List<User_Table>();

        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Deleted_userModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        public void OnGet()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            {
                string query = "SELECT * FROM User_Table WHERE account_approve = 1 AND isactive = 0";
                using SqlCommand command = new SqlCommand(query, connection);
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            User_Table user = new User_Table();

                            user.id = reader.GetInt32(0);
                            user.first_name = reader.GetString(1);
                            user.last_name = reader.GetString(2);
                            user.email = reader.GetString(3);
                            user.country = reader.GetString(7);
                            user.city = reader.GetString(8);
                            user.gender = reader.GetString(11);
                            user.role = reader.GetString(12);
                            if (!reader.IsDBNull(14))
                            {
                                user.profile_pic = reader.GetString(14);
                            }
                            else
                            {
                                user.profile_pic = null;
                            }

                            user.account_create_date = reader.GetDateTime(27);
                            User_Table.Add(user);
                        }
                    }

                    connection.Close();
                }
            }
        }

    }
}
