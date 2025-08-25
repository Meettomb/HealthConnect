using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Pharmacist_approvel
    {
        [Key]
        public int pharmacist_approvel_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string mobil_no { get; set; }
        public string dob { get; set; }
        public string House_number_and_Street_name { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string gender { get; set; }
        public string role { get; set; }
        public string password { get; set; }
        public string currency_code { get; set; }
        public string shop_licence { get; set; }
        public string shop_name { get; set; }
        public string shop_address { get; set; }
        public bool isactive { get; set; }
        public DateTime account_create_date { get; set; }
        public bool? account_approve { get; set; }
    }
}
