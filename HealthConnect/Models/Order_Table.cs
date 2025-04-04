namespace HealthConnect.Models
{
    public class Order_Table
    {
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
       
    }
}
