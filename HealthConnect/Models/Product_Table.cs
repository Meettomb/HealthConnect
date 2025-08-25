using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Product_Table
    {

        [Key]
        public int? product_id { get; set; }
        public int? saler_id { get; set; }
        public int? brande_id { get; set; }
        public string product_image { get; set; }
        public string product_name { get; set; }
        public int? product_category_id { get; set; }
        public int? product_price { get; set; }
        public int? product_discount { get; set; }
        public int? discounted_price { get; set; }
        public int? product_qantity { get; set; }
        public string product_description { get; set; }
        public string? product_features { get; set; }
        public string product_benefits { get; set; }
        public string product_how_to_use { get; set; }
        public string product_exp_date { get; set; }
        public DateOnly product_add_date { get; set; }

        public User_Table Seller { get; set; }
        public Pharmaceutical_Brands Brand { get; set; }
        public Medicine_Finel_Category FinelCategory { get; set; }
    }
}
