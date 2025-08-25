using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthConnect.Models
{
    public class Cart
    {
        [Key]
        public int cart_id { get; set; }

        public int user_id { get; set; }

        public int product_id { get; set; }
        public int quantity { get; set; }

        public Product_Table Product { get; set; }
        public User_Table Seller { get; set; }
        public Pharmaceutical_Brands Brand { get; set; }
        public Medicine_Finel_Category FinelCategory { get; set; }

    }
}
