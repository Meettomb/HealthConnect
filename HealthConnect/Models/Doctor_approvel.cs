using System.ComponentModel.DataAnnotations;

namespace HealthConnect.Models
{
    public class Doctor_approvel
    {
        [Key]
        public int doctor_approvel_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string mobil_no { get; set; }
        public string dob { get; set; }
        public string House_number_and_Street_name { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
        public string gender { get; set; }
        public string role { get; set; }
        public string password { get; set; }


        //      For Doctor
        public string profile_pic { get; set; }
        public string doctore_medical_license_photo { get; set; }
        public string medical_registration_no { get; set; }
        public string state_medical_council { get; set; }
        public string year_of_registration { get; set; }
        public string doctore_experience { get; set; }
        public string hospital_or_clinic { get; set; }
        public string doctor_qualifications { get; set; }
        public string doctor_type { get; set; }
        public string languages_spoken { get; set; }
        public string clinic_or_hospital_address { get; set; }
        public string on_site_consultation_fee { get; set; }
        public bool? account_approve { get; set; }
        public DateTime account_create_date { get; set; } = DateTime.Now;
        public bool isactive { get; set; }
        public string medicine_type { get; set; }
        public string currency_code { get; set; }
        public string video_call_consultation_fee { get; set; }
        public string? doctor_specialitis { get; set; }
    }
}
