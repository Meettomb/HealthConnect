using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace HealthConnect.Pages.User.Saler.Manage_Products
{
    public class Add_productsModel : PageModel
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



        public Add_productsModel(ILogger<IndexModel> logger, IConfiguration configuration)
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


        public async Task<IActionResult> OnPostAsync()
        {
            if (Product == null)
            {
                ErrorMessage = "Invalid product data.";
                return Page();
            }

            var uploadedFiles = Request.Form.Files;
            if (uploadedFiles.Count == 0)
            {
                ErrorMessage = "Please upload at least one product image.";
                return Page();
            }

            if (uploadedFiles.Count > 3)
            {
                ErrorMessage = "You can upload a maximum of 3 product images.";
                return Page();
            }

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImage");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            List<string> productImages = new List<string>();

            foreach (var file in uploadedFiles)
            {
                string originalFileName = Path.GetFileNameWithoutExtension(file.FileName).Replace(",", "");
                string extension = Path.GetExtension(file.FileName);
                string cleanedFileName = originalFileName + extension;

                string filePath = Path.Combine(uploadFolder, cleanedFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                productImages.Add(cleanedFileName);
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Product_Table 
            (saler_id, brande_id, product_name, product_description, product_price, 
            product_discount, product_exp_date, product_qantity, 
            product_category_id, product_image, product_add_date, product_features, 
            product_benefits, product_how_to_use, discounted_price) 
            VALUES 
            (@saler_id, @brande_id, @product_name, @product_description, @product_price, 
            @product_discount, @product_exp_date, @product_qantity, 
            @product_category_id, @product_image, @product_add_date, @product_features, 
            @product_benefits, @product_how_to_use, @discounted_price)";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    int originalPrice = Product.product_price ?? 0;
                    int? discount = Product.product_discount;
                    int discountedPrice = discount.HasValue
                        ? originalPrice - (originalPrice * discount.Value / 100)
                        : originalPrice;

                    cmd.Parameters.AddWithValue("@saler_id", Product.saler_id);
                    cmd.Parameters.AddWithValue("@brande_id", Product.brande_id);
                    cmd.Parameters.AddWithValue("@product_image", string.Join(",", productImages));
                    cmd.Parameters.AddWithValue("@product_name", Product.product_name);
                    cmd.Parameters.AddWithValue("@product_category_id", Product.product_category_id);
                    cmd.Parameters.AddWithValue("@product_price", Product.product_price);
                    cmd.Parameters.AddWithValue("@product_discount", Product.product_discount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@product_qantity", Product.product_qantity);
                    cmd.Parameters.AddWithValue("@product_description", Product.product_description);
                    cmd.Parameters.AddWithValue("@product_features", Product.product_features ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@product_benefits", Product.product_benefits);
                    cmd.Parameters.AddWithValue("@product_how_to_use", Product.product_how_to_use);
                    cmd.Parameters.AddWithValue("@product_exp_date", Product.product_exp_date);
                    cmd.Parameters.AddWithValue("@product_add_date", DateOnly.FromDateTime(DateTime.Now));
                    cmd.Parameters.AddWithValue("@discounted_price", discountedPrice);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    con.Close();

                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Product added successfully!";
                        SuccessMessage = "Product added successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to add product. Please try again.";
                        ErrorMessage = "Failed to add product. Please try again.";
                    }
                }
            }

            return RedirectToPage("/User/Saler/Manage_Products/Add_products");
        }


    }



}
