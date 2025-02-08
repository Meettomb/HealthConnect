using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Appointments
    {
        [Key]
        public int appointment_id {  get; set; }
        public int user_id {  get; set; }
        public int doctor_id {  get; set; }
        public string appointment_type { get; set; }
        public string time_slot { get; set; }
        public string appointment_date { get; set; }
        public bool appointment_approve { get; set; }
    }
}
