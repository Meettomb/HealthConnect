using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Order_Table
    {
        [Key]
        public int order_id { get; set; }
        public int user_id { get; set; }
        public int product_id { get; set; }
        public int seller_id { get; set; }
        public int price { get; set; }
        public string billing_address { get; set; }
        public int quantity { get; set; }
        public string paymant_methode { get; set; }
        public DateTime order_datetime { get; set; }
        public bool order_cancle { get; set; }
        public DateTime order_cancle_datetime { get; set; }
        public string order_status { get; set; }

        public User_Table Seller { get; set; }
        public User_Table User { get; set; }
        public Pharmaceutical_Brands Brand { get; set; }
        public Product_Table Product { get; set; }

        public Medicine_Finel_Category FinelCategory { get; set; }
    }
}
