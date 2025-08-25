using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Doctor_Specialitis
    {
        [Key]
        public int doctor_specialitis_id { get; set; }
        public int doctor_type_id { get; set; }

        public virtual Types_of_Doctor Types_of_Doctor { get; set; }
        public string doctor_specialitis { get; set; }


    }
}
