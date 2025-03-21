﻿namespace HealthConnect.Models
{
    public class Doctor_Report
    {
        public int doctor_report_id { get; set; }
        public int user_id { get; set; }
        public int doctor_id { get; set; }
        public string report_message { get; set; }

        public virtual User_Table UserId { get; set; }
        public virtual User_Table DoctorId { get; set; }
    }
}
