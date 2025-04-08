using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Asn1.Cmp;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HealthConnect.Pages.Ecom
{
    public class CartModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;


        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string House_number_and_Street_name { get; set; }
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


        public List<Product_Table> AllProductList { get; set; } = new List<Product_Table>();

        [BindProperty]
        public Product_Table Product { get; set; }

        [BindProperty]
        public Cart Cart { get; set; }

        public List<Cart> CartList { get; set; } = new List<Cart>();

        public int CartCount { get; set; }

        public CartModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId == null)
            {
                return RedirectToPage("/User/Sign_in");
            }
            OnGetLoginUserDetail();
            OnGetPharmacyCategory();
            OnCartGet(UserId.Value);
            OnGetCart();
            OnGetPharmacyAllProducts();
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
                                House_number_and_Street_name = reader["House_number_and_Street_name"].ToString();
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

        public void OnGetCart()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId == null) return;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT C.cart_id, C.user_id, C.product_id, C.quantity,
                PT.product_id, PT.saler_id, PT.brande_id, PT.product_image, PT.product_name,
                PT.product_category_id, PT.product_price, PT.product_discount, PT.discounted_price, 
                PT.product_qantity, PT.product_exp_date, PT.product_add_date,
                PB.pharmaceutical_brands_id, PB.pharmaceutical_brands_name,
                UT.id, UT.first_name, UT.last_name, UT.profile_pic, UT.shop_name, UT.shop_address,
                MFC.medicine_finel_category_id, MFC.medicine_finel_category_name,
                MSC.medicine_sub_category_id, MSC.medicine_sub_category_name, 
                MMC.medicine_main_category_id, MMC.medicine_main_category_name 
            FROM Cart C
            JOIN Product_Table PT ON C.product_id = PT.product_id
            JOIN Pharmaceutical_Brands PB ON PT.brande_id = PB.pharmaceutical_brands_id
            JOIN User_Table UT ON PT.saler_id = UT.id
            JOIN Medicine_Finel_Category MFC ON PT.product_category_id = MFC.medicine_finel_category_id
            JOIN Medicine_Sub_Category MSC ON MFC.medicine_sub_category_id = MSC.medicine_sub_category_id
            JOIN Medicine_Main_Category MMC ON MSC.medicine_main_category_id = MMC.medicine_main_category_id
            WHERE C.user_id = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId.Value);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cart cartItem = new Cart
                            {
                                cart_id = reader.GetInt32(0),
                                user_id = reader.GetInt32(1),
                                product_id = reader.GetInt32(2),
                                quantity = reader.GetInt32(3),

                                Product = new Product_Table
                                {
                                    product_id = reader.GetInt32(4),
                                    saler_id = reader.GetInt32(5),
                                    brande_id = reader.GetInt32(6),
                                    product_image = reader.GetString(7),
                                    product_name = reader.GetString(8),
                                    product_category_id = reader.GetInt32(9),
                                    product_price = reader.GetInt32(10),
                                    product_discount = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11),
                                    discounted_price = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                                    product_qantity = reader.GetInt32(13),
                                    product_exp_date = reader.GetString(14),
                                    product_add_date = DateOnly.FromDateTime(reader.GetDateTime(15)),

                                },

                                Brand = new Pharmaceutical_Brands
                                {
                                    pharmaceutical_brands_id = reader.GetInt32(16),
                                    pharmaceutical_brands_name = reader.GetString(17),
                                },

                                Seller = new User_Table
                                {
                                    id = reader.GetInt32(18),
                                    first_name = reader.GetString(19),
                                    last_name = reader.GetString(20),
                                    profile_pic = reader.IsDBNull(21) ? null : reader.GetString(21),
                                    shop_name = reader.GetString(22),
                                    shop_address = reader.GetString(23),
                                },

                                FinelCategory = new Medicine_Finel_Category
                                {
                                    medicine_finel_category_id = reader.GetInt32(24),
                                    medicine_finel_category_name = reader.GetString(25),

                                    Medicine_Sub_Category = new Medicine_Sub_Category
                                    {
                                        medicine_sub_category_id = reader.GetInt32(26),
                                        medicine_sub_category_name = reader.GetString(27),
                                    },

                                    Medicine_Main_Category = new Medicine_Main_Category
                                    {
                                        medicine_main_category_id = reader.GetInt32(28),
                                        medicine_main_category_name = reader.GetString(29),
                                    }
                                }
                            };

                            CartList.Add(cartItem);
                        }
                    }
                    con.Close();
                }
            }
        }
        private void OnGetPharmacyAllProducts()
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
        ORDER BY NEWID()";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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
                            AllProductList.Add(Product);
                        }
                    }
                }
            }
        }


        public IActionResult OnPost(string action, int cart_id, int quantity)
        {
            if (action == "remove")
            {
                return OnPostDelete(cart_id);
            }
            else if (action == "update")
            {
                return Updatecart(cart_id, quantity);
            }
            else if (action == "Pay")
            {
                return OnPostBuy();
            }

            return RedirectToPage("/Ecom/Cart");
        }

        public IActionResult OnPostDelete(int cart_id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Cart WHERE cart_id = @cart_id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@cart_id", cart_id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToPage("/Ecom/Cart");
        }

        public IActionResult Updatecart(int cart_id, int quantity)
        {
            string product_id = Request.Form["product_id"];
            int? UserId = HttpContext.Session.GetInt32("Id");

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                string checkQuery = "SELECT quantity FROM Cart WHERE user_id = @user_id AND product_id = @product_id";
                int cartQuantity = 0;

                using (SqlCommand checkCommand = new SqlCommand(checkQuery, con))
                {
                    checkCommand.Parameters.AddWithValue("@user_id", UserId.Value);
                    checkCommand.Parameters.AddWithValue("@product_id", product_id);
                    object result = checkCommand.ExecuteScalar();

                    if (result != null)
                    {
                        cartQuantity = Convert.ToInt32(result);
                    }
                }

                string productQuery = "SELECT product_qantity FROM Product_Table WHERE product_id = @product_id";
                int availableQuantity = 0;

                using (SqlCommand productCommand = new SqlCommand(productQuery, con))
                {
                    productCommand.Parameters.AddWithValue("@product_id", product_id);
                    object result = productCommand.ExecuteScalar();

                    if (result != null)
                    {
                        availableQuantity = Convert.ToInt32(result);
                    }
                }

                int requestedQuantity = Convert.ToInt32(Request.Form["quantity"]);
                int totalCartQuantity = cartQuantity + requestedQuantity;

                if (totalCartQuantity > availableQuantity)
                {
                    TempData["ErrorMessage"] = $"You can't add this product to cart. Available stock: {availableQuantity}.";
                    return Redirect("/Ecom/Cart");
                }


                string query = "UPDATE Cart SET quantity = @quantity WHERE cart_id = @cart_id";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@quantity", quantity);
                    cmd.Parameters.AddWithValue("@cart_id", cart_id);
                 
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Cart updated successfully!";
            return RedirectToPage("/Ecom/Cart");
        }

        public IActionResult OnPostBuy()
        {
            string productIdsStr = Request.Form["product_id"];
            string quantitiesStr = Request.Form["quantity"];
            string sellerIdsStr = Request.Form["seller_id"];

            string rawPrice = Request.Form["price"];
            string cleanedPrice = System.Text.RegularExpressions.Regex.Replace(rawPrice, @"[^\d]", "");
            int price = string.IsNullOrEmpty(cleanedPrice) ? 0 : Convert.ToInt32(cleanedPrice);

            string billingAddress = Request.Form["billing_address"];
            string paymentMethod = Request.Form["paymant_methode"];

            int? UserId = HttpContext.Session.GetInt32("Id");
            if (UserId == null)
            {
                return Redirect("/User/Sign_in");
            }

            string[] productIds = productIdsStr.Split(',');
            string[] quantities = quantitiesStr.Split(',');
            string[] sellerIds = sellerIdsStr.Split(',');

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                for (int i = 0; i < productIds.Length; i++)
                {
                    string productId = productIds[i].Trim();
                    int quantity = Convert.ToInt32(quantities[i].Trim());

                    string productQuery = "SELECT product_qantity, product_name FROM Product_Table WHERE product_id = @product_id";
                    int availableQuantity = 0;
                    string productName = "";

                    using (SqlCommand productCommand = new SqlCommand(productQuery, connection))
                    {
                        productCommand.Parameters.AddWithValue("@product_id", productId);
                        using (SqlDataReader reader = productCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                availableQuantity = Convert.ToInt32(reader["product_qantity"]);
                                productName = reader["product_name"].ToString();
                            }
                        }
                    }

                    if (quantity > availableQuantity)
                    {
                        TempData["ErrorMessage"] = $"Not enough stock for \"{productName}\". Available: {availableQuantity}.";
                        return Redirect("/Ecom/Cart");
                    }
                }

                for (int i = 0; i < productIds.Length; i++)
                {
                    string productId = productIds[i].Trim();
                    int quantity = Convert.ToInt32(quantities[i].Trim());
                    string sellerId = sellerIds[i].Trim();

                    string orderQuery = @"
                INSERT INTO Order_Table 
                (user_id, product_id, seller_id, price, billing_address, quantity, paymant_methode, order_datetime, order_cancle, order_status) 
                VALUES (@user_id, @product_id, @seller_id, @price, @billing_address, @quantity, @paymant_methode, @order_datetime, @order_cancle, @order_status)";

                    using (SqlCommand orderCommand = new SqlCommand(orderQuery, connection))
                    {
                        orderCommand.Parameters.AddWithValue("@user_id", UserId.Value);
                        orderCommand.Parameters.AddWithValue("@product_id", productId);
                        orderCommand.Parameters.AddWithValue("@seller_id", sellerId);
                        orderCommand.Parameters.AddWithValue("@price", price);
                        orderCommand.Parameters.AddWithValue("@billing_address", billingAddress);
                        orderCommand.Parameters.AddWithValue("@quantity", quantity);
                        orderCommand.Parameters.AddWithValue("@paymant_methode", paymentMethod);
                        orderCommand.Parameters.AddWithValue("@order_datetime", DateTime.Now);
                        orderCommand.Parameters.AddWithValue("@order_cancle", false);
                        orderCommand.Parameters.AddWithValue("@order_status", "Pending");

                        orderCommand.ExecuteNonQuery();
                    }

                    string reduceQuantityQuery = "UPDATE Product_Table SET product_qantity = product_qantity - @quantity WHERE product_id = @product_id";
                    using (SqlCommand reduceCommand = new SqlCommand(reduceQuantityQuery, connection))
                    {
                        reduceCommand.Parameters.AddWithValue("@product_id", productId);
                        reduceCommand.Parameters.AddWithValue("@quantity", quantity);
                        reduceCommand.ExecuteNonQuery();
                    }

                    string deleteCartQuery = "DELETE FROM Cart WHERE user_id = @user_id AND product_id = @product_id";
                    using (SqlCommand deleteCommand = new SqlCommand(deleteCartQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@user_id", UserId.Value);
                        deleteCommand.Parameters.AddWithValue("@product_id", productId);
                        deleteCommand.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Order placed successfully!";
                return Redirect("/Ecom/Cart");
            }
        }


    }
}
