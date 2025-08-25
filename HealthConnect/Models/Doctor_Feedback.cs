using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Doctor_Feedback
    {
        [Key]
        public int doctor_feedback_id { get; set; }
        public int user_id { get; set; }
        public int doctor_id { get; set; }
        public string feedback_message { get; set; }

        public virtual User_Table User { get; set; }
        public virtual User_Table DoctorUser { get; set; }
    }
}
