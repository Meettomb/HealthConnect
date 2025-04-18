using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;

namespace HealthConnect.Pages.Admin
{
    public class Admin_indexModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;


        public List<User_Table> User_Table = new List<User_Table>();

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public List<CountryCount> UserTable { get; set; }
        public List<Doctor_Report> DoctorReport = new List<Doctor_Report>();


        [BindProperty]
        public User_Table User { get; set; }

        public List<User_Table> User_TableList { get; set; } = new List<User_Table>();
        public List<User_Table> User_TableIdList { get; set; } = new List<User_Table>();


        [BindProperty]
        public Pharmaceutical_Brands Pharmaceutical_Brands { get; set; }
        public List<Pharmaceutical_Brands> Pharmaceutical_BrandsList { get; set; } = new List<Pharmaceutical_Brands>();

        [BindProperty]
        public Medicine_Main_Category Medicine_Main_Category { get; set; }
        public List<Medicine_Main_Category> Medicine_Main_CategoryList { get; set; } = new List<Medicine_Main_Category>();


        [BindProperty]
        public Medicine_Sub_Category Medicine_Sub_Category { get; set; }
        public List<Medicine_Sub_Category> Medicine_Sub_CategoryList { get; set; } = new List<Medicine_Sub_Category>();


        [BindProperty]
        public Medicine_Finel_Category Medicine_Finel_Category { get; set; }
        public List<Medicine_Finel_Category> Medicine_Finel_CategoryList { get; set; } = new List<Medicine_Finel_Category>();



        [BindProperty]
        public Product_Table Product_Table { get; set; }
        public List<Product_Table> Product_TableList { get; set; } = new List<Product_Table>();

        [BindProperty]
        public Product_Table Product { get; set; }

        [BindProperty]
        public Order_Table Order_Table { get; set; }
        public List<Order_Table> Order_TableList { get; set; } = new List<Order_Table>();
        public List<Feedback> FeedbackList { get; set; } = new List<Feedback>();

        public Admin_indexModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public Dictionary<string, int> CountryCounts { get; set; }

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

            GetTopFiveUser();
            GetUserCountsByCountry();
            GetTopFiveDoctorReports();
            OnGetTopFiveOrderList();
            OnGetFiveUserFeedback();

            return Page();

        }

        private void GetTopFiveUser()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 5 * FROM User_Table ORDER By account_create_date DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User_Table user = new User_Table
                            {
                                id = reader.GetInt32(0),
                                first_name = reader.GetString(1),
                                last_name = reader.GetString(2),
                                email = reader.GetString(3),
                                country = reader.GetString(7),
                                city = reader.GetString(8),
                                gender = reader.GetString(11),
                                role = reader.GetString(12),
                                profile_pic = reader.IsDBNull(14) ? null : reader.GetString(14),
                                account_create_date = reader.GetDateTime(27),
                                block = reader.IsDBNull(28) ? (bool?)null : reader.GetBoolean(28)
                            };
                            User_Table.Add(user);
                        }
                    }
                    con.Close();
                }
            }
        }

        public void GetUserCountsByCountry()
        {
            List<User_Table> userList = new List<User_Table>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM User_Table ORDER BY country"; 

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User_Table user = new User_Table
                            {
                                country = reader.GetString(7) 
                            };
                            userList.Add(user);
                        }
                    }
                }
            }

            var countryCounts = userList
                .GroupBy(u => u.country)
                .Select(g => new CountryCount { Country = g.Key, Count = g.Count() })
                .OrderBy(x => x.Country) 
                .ToList();

            foreach (var item in countryCounts)
            {
                Console.WriteLine($"{item.Country} = {item.Count}");
            }

            UserTable = countryCounts;
        }

        public void GetTopFiveDoctorReports()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT TOP 5 
                         DR.doctor_report_id, 
                         DR.user_id, 
                         DR.doctor_id, 
                         DR.report_message,
                         UT.id,
                         UT.first_name, 
                         UT.last_name, 
                         UT.email, 
                         UT.profile_pic                
                     FROM Doctor_Report DR
                     LEFT JOIN User_Table UT ON DR.user_id = UT.id
                     ORDER BY DR.doctor_report_id DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Doctor_Report report = new Doctor_Report
                            {
                                doctor_report_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                doctor_id = reader.GetInt32(2),
                                report_message = reader.GetString(3),

                                UserList = reader.IsDBNull(4) ? null : new User_Table
                                {
                                    id = reader.GetInt32(4),
                                    first_name = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    last_name = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    email = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    profile_pic = reader.IsDBNull(8) ? null : reader.GetString(8)
                                }
                            };
                            DoctorReport.Add(report);
                        }
                    }
                }
            }
        }
        private void OnGetTopFiveOrderList()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId == null) return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT TOP 5
                OT.order_id, OT.user_id, OT.product_id, OT.seller_id, OT.price, OT.billing_address, OT.quantity, OT.paymant_methode,
                OT.order_datetime, OT.order_cancle, OT.order_cancle_datetime, OT.order_status,

                Customer.id, Customer.first_name, Customer.last_name, Customer.profile_pic,
                Seller.id, Seller.first_name, Seller.last_name, Seller.profile_pic,

                PT.product_id, PT.product_name, PT.product_image, PT.product_price,

                MFC.medicine_finel_category_id, MFC.medicine_finel_category_name,
                MSC.medicine_sub_category_id, MSC.medicine_sub_category_name, 
                MMC.medicine_main_category_id, MMC.medicine_main_category_name,

                PB.pharmaceutical_brands_id, PB.pharmaceutical_brands_name

            FROM Order_Table OT
            LEFT JOIN User_Table Customer ON OT.user_id = Customer.id
            LEFT JOIN User_Table Seller ON OT.seller_id = Seller.id
            LEFT JOIN Product_Table PT ON OT.product_id = PT.product_id
            LEFT JOIN Medicine_Finel_Category MFC ON PT.product_category_id = MFC.medicine_finel_category_id
            LEFT JOIN Medicine_Sub_Category MSC ON PT.product_category_id = MSC.medicine_sub_category_id
            LEFT JOIN Medicine_Main_Category MMC ON MSC.medicine_main_category_id = MMC.medicine_main_category_id
            LEFT JOIN Pharmaceutical_Brands PB ON PT.brande_id = PB.pharmaceutical_brands_id
            ORDER BY OT.order_datetime DESC";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Order_Table order = new Order_Table
                            {
                                order_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                product_id = reader.GetInt32(2),
                                seller_id = reader.GetInt32(3),
                                price = reader.GetInt32(4),
                                billing_address = reader.GetString(5),
                                quantity = reader.GetInt32(6),
                                paymant_methode = reader.GetString(7),
                                order_datetime = reader.GetDateTime(8),
                                order_cancle = reader.GetBoolean(9),
                                order_cancle_datetime = reader.IsDBNull(10) ? DateTime.MinValue : reader.GetDateTime(10),
                                order_status = reader.GetString(11),
                                User = new User_Table
                                {
                                    id = reader.GetInt32(12),
                                    first_name = reader.GetString(13),
                                    last_name = reader.GetString(14),
                                    profile_pic = reader.IsDBNull(15) ? null : reader.GetString(15)
                                },
                                Seller = new User_Table
                                {
                                    id = reader.GetInt32(16),
                                    first_name = reader.GetString(17),
                                    last_name = reader.GetString(18),
                                    profile_pic = reader.IsDBNull(19) ? null : reader.GetString(19)
                                },
                                Product = new Product_Table
                                {
                                    product_id = reader.GetInt32(20),
                                    product_name = reader.GetString(21),
                                    product_image = reader.GetString(22),
                                    product_price = reader.GetInt32(23),
                                    FinelCategory = new Medicine_Finel_Category
                                    {
                                        medicine_finel_category_id = reader.IsDBNull(24) ? 0 : reader.GetInt32(24),
                                        medicine_finel_category_name = reader.IsDBNull(25) ? null : reader.GetString(25),
                                        Medicine_Sub_Category = new Medicine_Sub_Category
                                        {
                                            medicine_sub_category_id = reader.IsDBNull(26) ? 0 : reader.GetInt32(26),
                                            medicine_sub_category_name = reader.IsDBNull(27) ? null : reader.GetString(27)
                                        },
                                        Medicine_Main_Category = new Medicine_Main_Category
                                        {
                                            medicine_main_category_id = reader.IsDBNull(28) ? 0 : reader.GetInt32(28),
                                            medicine_main_category_name = reader.IsDBNull(29) ? null : reader.GetString(29)
                                        }
                                    }
                                },
                                Brand = new Pharmaceutical_Brands
                                {
                                    pharmaceutical_brands_id = reader.IsDBNull(30) ? 0 : reader.GetInt32(30),
                                    pharmaceutical_brands_name = reader.IsDBNull(31) ? null : reader.GetString(31)
                                }
                            };
                            Order_TableList.Add(order);
                        }
                    }
                }
            }
        }

        private void OnGetFiveUserFeedback()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT TOP 5 * FROM Feedback ORDER BY date DESC";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Feedback feedback = new Feedback
                            {
                                feedback_id = reader.GetInt32(0),
                                name = reader.GetString(1),
                                email = reader.GetString(2),
                                message = reader.GetString(3),
                                date = DateOnly.FromDateTime(reader.GetDateTime(4)),
                            };

                            FeedbackList.Add(feedback);
                        }
                    }
                }
                
            }
        }


        public class CountryCount
        {
            public string Country { get; set; }
            public int Count { get; set; }
        }



    }
}
