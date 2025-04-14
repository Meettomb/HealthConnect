using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthConnect.Models
{
    public class User_Table
    {
        [Key]
        public int id { get; set; }
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
        public string? profile_pic { get; set; }
        public string? doctore_medical_license_photo { get; set; }
        public string? medical_registration_no { get; set; }
        public string? state_medical_council { get; set; }
        public string? year_of_registration { get; set; }
        public string? doctore_experience { get; set; }
        public string? hospital_or_clinic { get; set; }
        public string? doctor_qualifications { get; set; }
        [ForeignKey("Types_of_Doctor")] public string? doctor_type { get; set; }
        public string? languages_spoken { get; set; }
        public string? clinic_or_hospital_address { get; set; }
        public string? on_site_consultation_fee { get; set; }
        public bool account_approve { get; set; }

        public DateTime account_create_date { get; set; }
        public bool? block { get; set; }
        public bool isactive { get; set; }
        public bool? mobail_verifie { get; set; }
        public string? auth_token { get; set; }
        public string? medicine_type { get; set; }
        public string? currency_code { get; set; }
        public string? video_call_consultation_fee { get; set; }
        public string? doctor_specialitis { get; set; }
        public bool? doctor_profile_complete { get; set; }

        public string? work_start_time { get; set; }
        public string? work_end_time { get; set; }
        public List<string>? weekly_work_days { get; set; }
        public string? max_time_per_appointments { get; set; }
        public string? break_between_two_appointments { get; set; }


        // For pharmasisit

        public string? shop_licence { get; set; }
        public string? shop_name { get; set; }
        public string? shop_address { get; set; }



        public virtual ICollection<Appointments> UserAppointments { get; set; } 
        public virtual ICollection<Appointments> DoctorAppointments { get; set; }

        public virtual ICollection<Doctor_Feedback> FeedbackList { get; set; }
        public double? AverageRating { get; set; }
        public int TotalFeedbackCount { get; set; }
        public virtual Types_of_Doctor Types_of_Doctor { get; set; }

    }

}

