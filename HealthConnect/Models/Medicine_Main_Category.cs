using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Medicine_Main_Category
    {
        [Key]
        public int medicine_main_category_id { get; set; }
        public string medicine_main_category_name { get; set; }
    }
}
