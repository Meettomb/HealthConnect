using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Medicine_Main_Category
    {
        [Key]
        public int medicine_main_category_id { get; set; }
        public string medicine_main_category_name { get; set; }

        public List<Medicine_Sub_Category> medicineSubCategories { get; set; } = new List<Medicine_Sub_Category>();

    }
}
