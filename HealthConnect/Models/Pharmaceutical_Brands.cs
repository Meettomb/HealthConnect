using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Pharmaceutical_Brands
    {
        [Key]
        public int pharmaceutical_brands_id { get; set; }
        public string pharmaceutical_brands_image { get; set; }
        public string pharmaceutical_brands_name { get; set; }
    }
}
