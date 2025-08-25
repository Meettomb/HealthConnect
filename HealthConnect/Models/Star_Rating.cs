using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Star_Rating
    {
        [Key]public int id { get; set; }
        public int doctor_id { get; set; }
        public int user_id { get; set; }
        public int rating { get; set; }

    }
}
