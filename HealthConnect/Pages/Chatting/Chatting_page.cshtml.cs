using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;

namespace HealthConnect.Pages.Chatting
{
    public class Chatting_pageModel : PageModel
    {
        private readonly ILogger<Chatting_pageModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        [BindProperty]
        public User_Table User { get; set; }

        public List<User_Table> User_list = new List<User_Table>();

        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string CurrencyCode { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

        public int? ReceiverId { get; set; }


        public Chatting_pageModel(ILogger<Chatting_pageModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int? id)
        {
            UserId = HttpContext.Session.GetInt32("Id");

            if (!UserId.HasValue)
            {
                return RedirectToPage("/User/Sign_in");
            }

            GetCurrentUserDetails(UserId.Value);
            GetDoctordetails(id ?? 0);

            LoadDoctorSpecialities();
            LoadDoctorTypes();
            ReceiverId = id;

            return Page();
        }

        private void GetCurrentUserDetails(int userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            FirstName = reader["first_name"].ToString();
                            LastName = reader["last_name"].ToString();
                            ProfilePic = reader["profile_pic"].ToString();
                            Role = reader["role"].ToString();
                            Country = reader["country"].ToString();
                            State = reader["state"].ToString();
                            City = reader["city"].ToString();
                            CurrencyCode = reader["currency_code"].ToString();
                        }
                    }
                }
            }
        }

        private void GetDoctordetails(int id)
        {
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
                            User = new User_Table
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
                                currency_code = !reader.IsDBNull(33) ? reader.GetString(33) : null,
                                doctor_specialitis = !reader.IsDBNull(35) ? reader.GetString(35) : null
                            };
                        }
                    }
                }
            }
        }

        private void LoadDoctorSpecialities()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT doctor_specialitis_id, doctor_specialitis FROM Doctor_Specialitis";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doctorSpecialitiesList.Add(new Doctor_Specialitis
                            {
                                doctor_specialitis_id = reader.GetInt32(0),
                                doctor_specialitis = reader.GetString(1),
                            });
                        }
                    }
                }
            }
        }

        private void LoadDoctorTypes()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT doctor_type_id, type_of_doctor FROM Types_of_Doctor";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Types_of_doctor.Add(new Types_of_Doctor
                            {
                                doctor_type_id = reader.GetInt32(0),
                                type_of_doctor = reader.GetString(1),
                            });
                        }
                    }
                }
            }
        }

    }
}
