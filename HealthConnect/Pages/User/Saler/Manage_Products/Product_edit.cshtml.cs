using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HealthConnect.Pages.User.Saler.Manage_Products
{
    public class Product_editModel : PageModel
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



        public Product_editModel(ILogger<IndexModel> logger, IConfiguration configuration)
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

                OnGetProductDetails(ProductId ?? 0);

            }
            else
            {
                return RedirectToPage("/index");
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Pharmaceutical_Brands";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pharmaceutical_Brands brand = new Pharmaceutical_Brands();
                            brand.pharmaceutical_brands_id = Convert.ToInt32(reader["pharmaceutical_brands_id"]);
                            brand.pharmaceutical_brands_image = reader["pharmaceutical_brands_image"].ToString();
                            brand.pharmaceutical_brands_name = reader["pharmaceutical_brands_name"].ToString();
                            Pharmaceutical_BrandsList.Add(brand);
                        }
                    }
                    con.Close();
                }
            }

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

            return Page();

        }

        private void OnGetProductDetails(int product_id)
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
                        Left JOIN Medicine_Finel_Category MFC ON PT.product_category_id = MFC.medicine_finel_category_id
                        LEFT JOIN Medicine_Sub_Category MSC ON MFC.medicine_sub_category_id = MSC.medicine_sub_category_id
                        LEFT JOIN Medicine_Main_Category MMC ON MSC.medicine_main_category_id = MMC.medicine_main_category_id
                        WHERE PT.product_id = @product_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@product_id", product_id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
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


        public async Task<IActionResult> OnPost(int product_id)
        {
            var productImages = Request.Form.Files;
            string newImages = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string selectQuery = "SELECT product_image FROM Product_Table WHERE product_id = @product_id";
                string oldImages = null;

                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@product_id", product_id);
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldImages = reader["product_image"]?.ToString();
                        }
                    }
                }

                if (productImages.Count > 0)
                {
                    List<string> imageFileNames = new List<string>();
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImage");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    for (int i = 0; i < Math.Min(productImages.Count, 3); i++)
                    {
                        var productImage = productImages[i];
                        string fileName = productImage.FileName.Replace(",", ""); // Remove commas
                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await productImage.CopyToAsync(fileStream);
                        }

                        imageFileNames.Add(fileName);
                    }

                    newImages = string.Join(",", imageFileNames); // Join with commas
                }
                else
                {
                    newImages = oldImages;
                }

                string query = @"UPDATE Product_Table SET 
        brande_id = @brande_id, 
        product_image = @product_image, 
        product_name = @product_name, 
        product_category_id = @product_category_id, 
        product_price = @product_price, 
        product_discount = @product_discount, 
        product_qantity = @product_qantity, 
        product_description = @product_description, 
        product_features = @product_features, 
        product_benefits = @product_benefits, 
        product_how_to_use = @product_how_to_use, 
        product_exp_date = @product_exp_date,
        discounted_price = @discounted_price
        WHERE product_id = @product_id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    int originalPrice = Product.product_price ?? 0;
                    int? discount = Product.product_discount;
                    int discountedPrice = discount.HasValue
                        ? originalPrice - (originalPrice * discount.Value / 100)
                        : originalPrice;

                    command.Parameters.AddWithValue("@product_id", product_id);
                    command.Parameters.AddWithValue("@brande_id", Request.Form["brande_id"].ToString());
                    command.Parameters.AddWithValue("@product_name", Request.Form["product_name"].ToString());
                    command.Parameters.AddWithValue("@product_category_id", Request.Form["product_category_id"].ToString());
                    command.Parameters.AddWithValue("@product_price", Request.Form["product_price"].ToString());
                    command.Parameters.AddWithValue("@product_discount", string.IsNullOrEmpty(Request.Form["product_discount"]) ? (object)DBNull.Value : Request.Form["product_discount"].ToString());
                    command.Parameters.AddWithValue("@product_qantity", Request.Form["product_qantity"].ToString());
                    command.Parameters.AddWithValue("@product_description", Request.Form["product_description"].ToString());
                    command.Parameters.AddWithValue("@product_features", Request.Form["product_features"].ToString());
                    command.Parameters.AddWithValue("@product_benefits", Request.Form["product_benefits"].ToString());
                    command.Parameters.AddWithValue("@product_how_to_use", Request.Form["product_how_to_use"].ToString());
                    command.Parameters.AddWithValue("@product_exp_date", Request.Form["product_exp_date"].ToString());

                    command.Parameters.AddWithValue("@product_image", string.IsNullOrEmpty(newImages) ? (object)DBNull.Value : newImages);
                    command.Parameters.AddWithValue("@discounted_price", discountedPrice);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        TempData["SuccessMessage"] = "Product details updated successfully!";
                        return RedirectToPage("/User/Saler/Manage_Products/View_products");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Something went wrong. Please try again.";
                    }
                }
            }

            return Page();
        }



    }
}
