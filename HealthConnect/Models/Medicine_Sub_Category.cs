using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Medicine_Sub_Category
    {
       [Key] 
        public int medicine_sub_category_id { get; set; }
        public int medicine_main_category_id { get; set; }

        public string medicine_sub_category_name { get; set; }

        public virtual Medicine_Main_Category Medicine_Main_Category { get; set; }

    }
}
