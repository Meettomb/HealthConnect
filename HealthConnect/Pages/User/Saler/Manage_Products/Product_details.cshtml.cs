using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace HealthConnect.Pages.User.Saler.Manage_Products
{
    public class Product_detailsModel : PageModel
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
        public bool? DoctorProfileComplete { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

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



        public Product_detailsModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet(int product_id)
        {

            string roleInSession = HttpContext.Session.GetString("UserRole");

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
                                FirstName = reader["first_name"]?.ToString();
                                LastName = reader["last_name"]?.ToString();
                                ProfilePic = reader["profile_pic"]?.ToString();
                                Role = reader["role"]?.ToString();
                                UserId = reader["id"] as int?;
                            }
                            else
                            {
                                ErrorMessage = "User not found.";
                            }
                        }
                        con.Close();
                    }
                }


                if (Role != "Pharmacist")
                {
                    return RedirectToPage("/index");
                }

                int? ProductId = null;
                if (Request.Query.ContainsKey("product_id") && int.TryParse(Request.Query["product_id"], out int parsedProductId))
                {
                    ProductId = parsedProductId;
                }

                if (UserId.HasValue)
                {
                    OnGetProducts(UserId.Value, ProductId);
                }

            }
            else
            {
                return RedirectToPage("/index");
            }

            return Page();

        }

        private void OnGetProducts(int UserId, int? product_id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT PT.product_id, PT.saler_id, PT.brande_id, PT.product_image, PT.product_name,
                PT.product_category_id, PT.product_price, PT.product_discount, PT.discounted_price, PT.product_qantity, 
                PT.product_description, PT.product_features, PT.product_benefits, PT.product_how_to_use,
                PT.product_exp_date, PT.product_add_date, PB.pharmaceutical_brands_id, PB.pharmaceutical_brands_name,
                UT.id, UT.first_name, UT.last_name, UT.profile_pic, UT.shop_name, UT.shop_address,
                MFC.medicine_finel_category_id, MFC.medicine_finel_category_name,
                MSC.medicine_sub_category_id, MSC.medicine_sub_category_name, 
                MMC.medicine_main_category_id, MMC.medicine_main_category_name
                FROM Product_Table PT
                LEFT JOIN Pharmaceutical_Brands PB ON PT.brande_id = PB.pharmaceutical_brands_id
                LEFT JOIN User_Table UT ON PT.saler_id = UT.id
                LEFT JOIN Medicine_Finel_Category MFC ON PT.product_category_id = MFC.medicine_finel_category_id
                LEFT JOIN Medicine_Sub_Category MSC ON MFC.medicine_sub_category_id = MSC.medicine_sub_category_id
                LEFT JOIN Medicine_Main_Category MMC ON MSC.medicine_main_category_id = MMC.medicine_main_category_id
                WHERE (@product_id IS NOT NULL AND PT.product_id = @product_id) 
                      OR (@product_id IS NULL AND PT.saler_id = @UserId)";
        
        using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", UserId);
                    command.Parameters.AddWithValue("@product_id", (object)product_id ?? DBNull.Value);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"Fetched product_id: {reader.GetInt32(0)}"); 

                            Product = new Product_Table
                            {
                                product_id = reader.GetInt32(0),
                                saler_id = reader.GetInt32(1),
                                brande_id = reader.GetInt32(2),
                                product_image = reader.GetString(3),
                                product_name = reader.GetString(4),
                                product_category_id = reader.GetInt32(5),
                                product_price = reader.GetInt32(6),
                                product_discount = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                                discounted_price = reader.IsDBNull(8) ? (int?)null : reader.GetInt32(8),
                                product_qantity = reader.GetInt32(9),
                                product_description = reader.GetString(10),
                                product_features = reader.IsDBNull(11) ? null : reader.GetString(11),
                                product_benefits = reader.GetString(12),
                                product_how_to_use = reader.GetString(13),
                                product_exp_date = reader.GetString(14),
                                product_add_date = DateOnly.FromDateTime(reader.GetDateTime(15)),

                                Brand = new Pharmaceutical_Brands
                                {
                                    pharmaceutical_brands_id = reader.GetInt32(16),
                                    pharmaceutical_brands_name = reader.GetString(17)
                                },

                                Seller = new User_Table
                                {
                                    id = reader.GetInt32(18),
                                    first_name = reader.GetString(19),
                                    last_name = reader.GetString(20),
                                    profile_pic = reader.IsDBNull(21) ? null : reader.GetString(21),
                                    shop_name = reader.IsDBNull(22) ? null : reader.GetString(22),
                                    shop_address = reader.IsDBNull(23) ? null : reader.GetString(23)
                                },

                                FinelCategory = new Medicine_Finel_Category
                                {
                                    medicine_finel_category_id = reader.GetInt32(24),
                                    medicine_finel_category_name = reader.GetString(25),

                                    Medicine_Sub_Category = new Medicine_Sub_Category
                                    {
                                        medicine_sub_category_id = reader.GetInt32(26),
                                        medicine_sub_category_name = reader.GetString(27)
                                    },

                                    Medicine_Main_Category = new Medicine_Main_Category
                                    {
                                        medicine_main_category_id = reader.GetInt32(28),
                                        medicine_main_category_name = reader.GetString(29)
                                    }
                                }
                            };
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            if (Request.Form.ContainsKey("product_id"))
            {
                if (int.TryParse(Request.Form["product_id"], out int productId))
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // Delete from Cart table
                                string deleteCartQuery = "DELETE FROM Cart WHERE product_id = @product_id";
                                using (SqlCommand cartCommand = new SqlCommand(deleteCartQuery, connection, transaction))
                                {
                                    cartCommand.Parameters.AddWithValue("@product_id", productId);
                                    cartCommand.ExecuteNonQuery();
                                }

                                // Delete from Product_Table
                                string deleteProductQuery = "DELETE FROM Product_Table WHERE product_id = @product_id";
                                using (SqlCommand productCommand = new SqlCommand(deleteProductQuery, connection, transaction))
                                {
                                    productCommand.Parameters.AddWithValue("@product_id", productId);
                                    productCommand.ExecuteNonQuery();
                                }

                                // Commit the transaction
                                transaction.Commit();
                                TempData["SuccessMessage"] = "Product and associated cart items deleted successfully.";
                            }
                            catch (Exception ex)
                            {
                                // Rollback if an error occurs
                                transaction.Rollback();
                                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
                                Console.WriteLine(ex.Message); // Log error for debugging
                            }
                        }
                    }

                    return RedirectToPage("/User/Saler/Manage_Products/View_products");
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid Product ID.";
                    return RedirectToPage("/User/Saler/Manage_Products/View_products");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Product ID is missing.";
                return RedirectToPage("/User/Saler/Manage_Products/View_products");
            }
        }



    }
}
