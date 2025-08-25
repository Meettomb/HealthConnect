using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Medicine_Finel_Category
    {
        [Key]
        public int medicine_finel_category_id { get; set; }
        public int medicine_main_category_id { get; set; }
        public int medicine_sub_category_id { get; set; }
        public string medicine_finel_category_name { get; set; }


        public virtual Medicine_Main_Category Medicine_Main_Category { get; set; }
        public virtual Medicine_Sub_Category Medicine_Sub_Category { get; set; }
    }
}
