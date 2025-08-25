using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthConnect.Models
{
    public class Appointments
    {
        [Key]
        public int appointment_id { get; set; }

        [ForeignKey("User")]
        public int user_id { get; set; }

        [ForeignKey("Doctor")]
        public int doctor_id { get; set; }

        public string appointment_type { get; set; }
        public string time_slot { get; set; }
        public string appointment_date { get; set; }
        public bool appointment_approve { get; set; }
        public DateTime book_date_time { get; set; }
        public string booking_user_role { get; set; }
        public string? problem { get; set; }
        public bool appointment_cancel { get; set; }


        public virtual User_Table User { get; set; }

        public virtual User_Table Doctor { get; set; }

    }
}
