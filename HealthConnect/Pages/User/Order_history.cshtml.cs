using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace HealthConnect.Pages.User
{
    public class Order_historyModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;


        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public bool? DoctorProfileComplete { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }


        [BindProperty]
        public User_Table User { get; set; }

        public List<User_Table> User_TableList { get; set; } = new List<User_Table>();
        public List<User_Table> User_TableIdList { get; set; } = new List<User_Table>();


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
        public List<Product_Table> FifityDiscountProductList { get; set; } = new List<Product_Table>();

        [BindProperty]
        public Product_Table Product { get; set; }

        [BindProperty]
        public Cart Cart { get; set; }


        [BindProperty]
        public Order_Table Order_Table { get; set; }
        public List<Order_Table> Order_TableList { get; set; } = new List<Order_Table>();
        public int CartCount { get; set; }

        public Order_historyModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");
            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {

                OnGetLoginUserDetail();
                OnCartGet(UserId.Value);
                OnGetOrderList();
            }

            OnGetPharmacyCategory();
            return Page();
        }

        public void OnCartGet(int user_id)
        {
            int cartCount = 0;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string getDoctorCount = "SELECT COUNT(*) FROM Cart WHERE user_id = @UserId";
                using (SqlCommand command = new SqlCommand(getDoctorCount, connection))
                {
                    command.Parameters.AddWithValue("@UserId", user_id);
                    CartCount = (int)command.ExecuteScalar();
                }

            }
        }
        public void OnGetLoginUserDetail()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                if (UserId.HasValue)
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
                                Country = reader["country"].ToString();
                                State = reader["state"].ToString();
                                City = reader["city"].ToString();
                                Pincode = reader["pincode"].ToString();
                                UserId = (int?)reader["id"];
                                DoctorProfileComplete = reader["doctor_profile_complete"] != DBNull.Value ? (bool?)(Convert.ToInt32(reader["doctor_profile_complete"]) == 1) : null;


                            }
                        }
                        con.Close();
                    }
                }
            }

        }
        public void OnGetPharmacyCategory()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                SELECT 
                    MM.medicine_main_category_id,
                    MM.medicine_main_category_name,
                    MS.medicine_sub_category_id,
                    MS.medicine_sub_category_name,
                    MF.medicine_finel_category_id,
                    MF.medicine_finel_category_name
                FROM 
                    Medicine_Main_Category MM
                LEFT JOIN 
                    Medicine_Sub_Category MS 
                ON 
                    MM.medicine_main_category_id = MS.medicine_main_category_id
                LEFT JOIN 
                    Medicine_Finel_Category MF 
                ON 
                    MS.medicine_sub_category_id = MF.medicine_sub_category_id
                ORDER BY 
                    MM.medicine_main_category_id, 
                    MS.medicine_sub_category_id,
                    MF.medicine_finel_category_id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        var mainCategoryDict = new Dictionary<int, Medicine_Main_Category>();

                        while (reader.Read())
                        {
                            int mainId = reader.GetInt32(0);
                            string mainName = reader.GetString(1);
                            int subId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            string subName = reader.IsDBNull(3) ? null : reader.GetString(3);
                            int finelId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                            string finelName = reader.IsDBNull(5) ? null : reader.GetString(5);

                            if (!mainCategoryDict.ContainsKey(mainId))
                            {
                                mainCategoryDict[mainId] = new Medicine_Main_Category
                                {
                                    medicine_main_category_id = mainId,
                                    medicine_main_category_name = mainName,
                                    medicineSubCategories = new List<Medicine_Sub_Category>()
                                };
                            }

                            if (subId != 0)
                            {
                                var mainCategory = mainCategoryDict[mainId];
                                var subCategory = mainCategory.medicineSubCategories.FirstOrDefault(s => s.medicine_sub_category_id == subId);

                                if (subCategory == null)
                                {
                                    subCategory = new Medicine_Sub_Category
                                    {
                                        medicine_sub_category_id = subId,
                                        medicine_sub_category_name = subName,
                                        medicine_main_category_id = mainId,
                                        Medicine_Finel_Categories = new List<Medicine_Finel_Category>()
                                    };
                                    mainCategory.medicineSubCategories.Add(subCategory);
                                }

                                if (finelId != 0)
                                {
                                    subCategory.Medicine_Finel_Categories ??= new List<Medicine_Finel_Category>();
                                    subCategory.Medicine_Finel_Categories.Add(new Medicine_Finel_Category
                                    {
                                        medicine_finel_category_id = finelId,
                                        medicine_finel_category_name = finelName,
                                        medicine_main_category_id = mainId,
                                        medicine_sub_category_id = subId
                                    });
                                }
                            }
                        }

                        Medicine_Main_CategoryList = mainCategoryDict.Values.ToList();
                    }
                }
            }
        }
        private void OnGetOrderList()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId == null) return;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT OT.order_id, OT.user_id, OT.product_id, OT.seller_id, OT.price, OT.billing_address, OT.quantity, OT.paymant_methode,
                  OT.order_datetime, OT.order_cancle, OT.order_cancle_datetime, OT.order_status,
                  Customer.id, Customer.first_name, Customer.last_name, Customer.profile_pic,
                  PT.product_id, PT.product_name, PT.product_image, PT.product_price, 
                  MFC.medicine_finel_category_id, MFC.medicine_finel_category_name,
                  MSC.medicine_sub_category_id, MSC.medicine_sub_category_name, 
                  MMC.medicine_main_category_id, MMC.medicine_main_category_name,
                  PB.pharmaceutical_brands_id, PB.pharmaceutical_brands_name
                  FROM Order_Table OT
                  LEFT JOIN User_Table Customer ON OT.user_id = Customer.id
                  LEFT JOIN Product_Table PT ON OT.product_id = PT.product_id
                  LEFT JOIN Medicine_Finel_Category MFC ON PT.product_category_id = MFC.medicine_finel_category_id
                  LEFT JOIN Medicine_Sub_Category MSC ON PT.product_category_id = MSC.medicine_sub_category_id
                  LEFT JOIN Medicine_Main_Category MMC ON MSC.medicine_main_category_id = MMC.medicine_main_category_id
                  LEFT JOIN Pharmaceutical_Brands PB ON PT.brande_id = PB.pharmaceutical_brands_id
                    WHERE OT.user_id = @UserId
                    ORDER BY OT.order_datetime DESC";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
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
                                    profile_pic = reader.IsDBNull(15) ? null : reader.GetString(15),
                                },
                                Product = new Product_Table
                                {
                                    product_id = reader.GetInt32(16),
                                    product_name = reader.GetString(17),
                                    product_image = reader.GetString(18),
                                    product_price = reader.GetInt32(19),
                                    FinelCategory = new Medicine_Finel_Category
                                    {
                                        medicine_finel_category_id = reader.IsDBNull(20) ? 0 : reader.GetInt32(20),
                                        medicine_finel_category_name = reader.IsDBNull(21) ? null : reader.GetString(21),
                                        Medicine_Sub_Category = new Medicine_Sub_Category
                                        {
                                            medicine_sub_category_id = reader.IsDBNull(22) ? 0 : reader.GetInt32(22),
                                            medicine_sub_category_name = reader.IsDBNull(23) ? null : reader.GetString(23)
                                        },
                                        Medicine_Main_Category = new Medicine_Main_Category
                                        {
                                            medicine_main_category_id = reader.IsDBNull(24) ? 0 : reader.GetInt32(24),
                                            medicine_main_category_name = reader.IsDBNull(25) ? null : reader.GetString(25)
                                        }
                                    },
                                },
                                Brand = new Pharmaceutical_Brands
                                {
                                    pharmaceutical_brands_id = reader.IsDBNull(26) ? 0 : reader.GetInt32(26),
                                    pharmaceutical_brands_name = reader.IsDBNull(27) ? null : reader.GetString(27)
                                }
                            };
                            Order_TableList.Add(order);
                        }
                    }
                }
            }
        }





    }
}
