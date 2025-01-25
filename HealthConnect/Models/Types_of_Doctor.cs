using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Types_of_Doctor
    {
        [Key]
        public int doctor_type_id { get; set; }
        public string type_of_doctor { get; set; }
    }
}
