using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net.Mail;

namespace HealthConnect.Pages.Admin.UserData
{
    public class Register_inquiry_approveModel : PageModel
    {

        [BindProperty]
        public Doctor_approvel Doctor_approvel { get; set; }

        public string TypeOfDoctor { get; set; }


        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string ProfilePic { get; set; }

        public string? ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public Register_inquiry_approveModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }
        public int? UserId { get; set; }

        public IActionResult OnGet(int doctor_approvel_id)
        {
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId.HasValue)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM User_Table WHERE id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserId.Value);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                FirstName = reader["first_name"].ToString();
                                LastName = reader["last_name"].ToString();
                                ProfilePic = reader["profile_pic"].ToString();
                                Role = reader["role"].ToString();
                                UserId = reader["id"] as int?;
                            }
                        }
                        con.Close();
                    }
                }

                if (Role != "Admin")
                {
                    return RedirectToPage("/index");
                }
            }
            else
            {
                return RedirectToPage("/index");
            }
            Doctor_approvel doctor = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string doctorQuery = "SELECT * FROM Doctor_approvel WHERE doctor_approvel_id=@doctor_approvel_id";
                string typeQuery = "SELECT type_of_doctor FROM Types_of_Doctor WHERE doctor_type_id=@doctor_type_id";


                using (SqlCommand doctorCommand = new SqlCommand(doctorQuery, connection))
                {
                    doctorCommand.Parameters.AddWithValue("@doctor_approvel_id", doctor_approvel_id);
                    connection.Open();

                    using (SqlDataReader doctorReader = doctorCommand.ExecuteReader())
                    {
                        if (doctorReader.Read())
                        {
                            doctor = new Doctor_approvel
                            {
                                doctor_approvel_id = doctorReader.GetInt32(0),
                                first_name = doctorReader.GetString(1),
                                last_name = doctorReader.GetString(2),
                                email = doctorReader.GetString(3),
                                mobil_no = doctorReader.GetString(4),
                                dob = doctorReader.GetString(5),
                                House_number_and_Street_name = doctorReader.GetString(6),
                                country = doctorReader.GetString(7),
                                city = doctorReader.GetString(8),
                                state = doctorReader.GetString(9),
                                pincode = doctorReader.GetString(10),
                                gender = doctorReader.GetString(11),
                                role = doctorReader.GetString(12),
                                password = doctorReader.GetString(13),
                                profile_pic = doctorReader.GetString(14),
                                doctore_medical_license_photo = doctorReader.GetString(15),
                                medical_registration_no = doctorReader.GetString(16),
                                state_medical_council = doctorReader.GetString(17),
                                year_of_registration = doctorReader.GetString(18),
                                doctore_experience = doctorReader.GetString(19),
                                hospital_or_clinic = doctorReader.GetString(20),
                                doctor_qualifications = doctorReader.GetString(21),
                                languages_spoken = doctorReader.GetString(23),
                                clinic_or_hospital_address = doctorReader.GetString(24),
                                on_site_consultation_fee = doctorReader.GetString(25),
                                account_approve = !doctorReader.IsDBNull(26) ? doctorReader.GetBoolean(26) : (bool?)null,
                                account_create_date = !doctorReader.IsDBNull(27) ? doctorReader.GetDateTime(27) : DateTime.Now,
                                isactive = doctorReader.GetBoolean(28),
                                medicine_type = doctorReader.GetString(29),
                                currency_code = doctorReader.GetString(30),
                                video_call_consultation_fee = doctorReader.GetString(31),
                                doctor_type = doctorReader.IsDBNull(22) ? null : doctorReader.GetString(22),

                                doctor_specialitis = doctorReader.GetString(32)
                            };
                        }
                    }

                    if (doctor != null && int.TryParse(doctor.doctor_type, out int doctorTypeId))
                    {
                        using (SqlCommand typeCommand = new SqlCommand(typeQuery, connection))
                        {
                            typeCommand.Parameters.AddWithValue("@doctor_type_id", doctorTypeId);
                            using (SqlDataReader typeReader = typeCommand.ExecuteReader())
                            {
                                if (typeReader.Read())
                                {
                                    doctor.doctor_type = typeReader.GetString(0);
                                }
                            }
                        }
                    }

                    if (doctor != null && !string.IsNullOrEmpty(doctor.doctor_specialitis))
                    {
                        var specialitisIds = doctor.doctor_specialitis
                            .Split(',')
                            .Select(id => $"'{id.Trim()}'")
                            .ToList();

                        string specialitisNameQuery = $"SELECT doctor_specialitis FROM Doctor_Specialitis WHERE doctor_specialitis_id IN ({string.Join(",", specialitisIds)})";

                        using (SqlCommand specialitisCommand = new SqlCommand(specialitisNameQuery, connection))
                        {
                            using (SqlDataReader specialitisReader = specialitisCommand.ExecuteReader())
                            {
                                var specialitisNames = new List<string>();
                                while (specialitisReader.Read())
                                {
                                    specialitisNames.Add(specialitisReader.GetString(0)); 
                                }
                                doctor.doctor_specialitis = string.Join(", ", specialitisNames);
                            }
                        }
                    }

                }
            }


            if (doctor == null)
            {
                return NotFound();
            }

            Doctor_approvel = doctor;
            return Page();
        }


        public IActionResult OnPostApprove(int doctor_approvel_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM Doctor_approvel WHERE doctor_approvel_id = @Id", connection);
                command.Parameters.AddWithValue("@Id", doctor_approvel_id);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var doctor = new User_Table
                    {
                        first_name = reader.GetString(1),
                        last_name = reader.GetString(2),
                        email = reader.GetString(3),
                        mobil_no = reader.GetString(4),
                        dob = reader.GetString(5),
                        House_number_and_Street_name = reader.GetString(6),
                        country = reader.GetString(7),
                        city = reader.GetString(8),
                        state = reader.GetString(9),
                        pincode = reader.GetString(10),
                        gender = reader.GetString(11),
                        role = reader.GetString(12),
                        password = reader.GetString(13),
                        profile_pic = reader.IsDBNull(14) ? null : reader.GetString(14),
                        doctore_medical_license_photo = reader.IsDBNull(15) ? null : reader.GetString(15),
                        medical_registration_no = reader.IsDBNull(16) ? null : reader.GetString(16),
                        state_medical_council = reader.IsDBNull(17) ? null : reader.GetString(17),
                        year_of_registration = reader.IsDBNull(18) ? null : reader.GetString(18),
                        doctore_experience = reader.IsDBNull(19) ? null : reader.GetString(19),
                        hospital_or_clinic = reader.IsDBNull(20) ? null : reader.GetString(20),
                        doctor_qualifications = reader.IsDBNull(21) ? null : reader.GetString(21),
                        doctor_type = reader.IsDBNull(22) ? null : reader.GetString(22),

                        languages_spoken = reader.IsDBNull(23) ? null : reader.GetString(23),
                        clinic_or_hospital_address = reader.IsDBNull(24) ? null : reader.GetString(24),
                        on_site_consultation_fee = reader.IsDBNull(25) ? null : reader.GetString(25),
                        account_approve = true,
                        account_create_date = DateTime.Now,
                        isactive = true,
                        block = false,
                        mobail_verifie = false,
                        medicine_type = reader.IsDBNull(29) ? null : reader.GetString(29),
                        currency_code = reader.IsDBNull(30) ? null : reader.GetString(30),
                        video_call_consultation_fee = reader.IsDBNull(31) ? null : reader.GetString(31),
                        doctor_specialitis = reader.IsDBNull(32) ? null : reader.GetString(32)
                    };
                    reader.Close();

                    var insertCommand = new SqlCommand("INSERT INTO User_Table (first_name, last_name, email, mobil_no, dob, House_number_and_Street_name, country, city, state, pincode, gender, role, password, profile_pic, doctore_medical_license_photo, medical_registration_no, state_medical_council, year_of_registration, doctore_experience, hospital_or_clinic, doctor_qualifications, doctor_type, languages_spoken, clinic_or_hospital_address, on_site_consultation_fee, account_approve, account_create_date, isactive, block, mobail_verifie, medicine_type, currency_code, video_call_consultation_fee, doctor_specialitis, doctor_profile_complete) VALUES (@FirstName, @LastName, @Email, @MobileNo, @Dob, @Address, @Country, @City, @State, @Pincode, @Gender, @Role, @Password, @ProfilePic, @MedicalLicensePhoto, @MedicalRegNo, @StateMedicalCouncil, @YearOfRegistration, @Experience, @HospitalClinic, @Qualifications, @DoctorType, @Languages, @ClinicAddress, @On_site_consultation_fee, @Approve, @CreateDate, @IsActive, @Block, @Mobail_verifie, @Medicine_type, @Currency_code, @Video_call_consultation_fee, @Doctor_specialitis, @Doctor_profile_complete)", connection);
                    insertCommand.Parameters.AddWithValue("@FirstName", doctor.first_name);
                    insertCommand.Parameters.AddWithValue("@LastName", doctor.last_name);
                    insertCommand.Parameters.AddWithValue("@Email", doctor.email);
                    insertCommand.Parameters.AddWithValue("@MobileNo", doctor.mobil_no);
                    insertCommand.Parameters.AddWithValue("@Dob", doctor.dob);
                    insertCommand.Parameters.AddWithValue("@Address", doctor.House_number_and_Street_name);
                    insertCommand.Parameters.AddWithValue("@Country", doctor.country);
                    insertCommand.Parameters.AddWithValue("@City", doctor.city);
                    insertCommand.Parameters.AddWithValue("@State", doctor.state);
                    insertCommand.Parameters.AddWithValue("@Pincode", doctor.pincode);
                    insertCommand.Parameters.AddWithValue("@Gender", doctor.gender);
                    insertCommand.Parameters.AddWithValue("@Role", doctor.role);
                    insertCommand.Parameters.AddWithValue("@Password", doctor.password);
                    insertCommand.Parameters.AddWithValue("@ProfilePic", (object)doctor.profile_pic ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@MedicalLicensePhoto", (object)doctor.doctore_medical_license_photo ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@MedicalRegNo", (object)doctor.medical_registration_no ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@StateMedicalCouncil", (object)doctor.state_medical_council ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@YearOfRegistration", (object)doctor.year_of_registration ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Experience", (object)doctor.doctore_experience ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@HospitalClinic", (object)doctor.hospital_or_clinic ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Qualifications", (object)doctor.doctor_qualifications ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@DoctorType", (object)doctor.doctor_type ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Languages", (object)doctor.languages_spoken ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@ClinicAddress", (object)doctor.clinic_or_hospital_address ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@On_site_consultation_fee", (object)doctor.on_site_consultation_fee ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Approve", doctor.account_approve);
                    insertCommand.Parameters.AddWithValue("@CreateDate", doctor.account_create_date);
                    insertCommand.Parameters.AddWithValue("@IsActive", doctor.isactive);
                    insertCommand.Parameters.AddWithValue("@Block", doctor.block);
                    insertCommand.Parameters.AddWithValue("@Mobail_verifie", doctor.mobail_verifie);
                    insertCommand.Parameters.AddWithValue("@Medicine_type", (object)doctor.medicine_type ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Currency_code", (object)doctor.currency_code ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Video_call_consultation_fee", (object)doctor.video_call_consultation_fee ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@Doctor_specialitis", (object)doctor.doctor_specialitis ?? DBNull.Value);
                    insertCommand.Parameters.AddWithValue("@doctor_profile_complete", false);

                    insertCommand.ExecuteNonQuery();

                    var updateCommand = new SqlCommand("UPDATE Doctor_approvel SET isactive = @IsActive, account_approve = @Approve WHERE doctor_approvel_id = @Id", connection);
                    updateCommand.Parameters.AddWithValue("@IsActive", true);
                    updateCommand.Parameters.AddWithValue("@Approve", true);
                    updateCommand.Parameters.AddWithValue("@Id", doctor_approvel_id);
                    updateCommand.ExecuteNonQuery();

                    SendApprovalEmail(doctor.email);  
                }
            }
            return RedirectToPage("/Admin/UserData/Register_inquiry");
        }

        public IActionResult OnPostReject(int doctor_approvel_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Doctor_approvel SET account_approve = @Approve WHERE doctor_approvel_id = @Id", connection);

                command.Parameters.AddWithValue("@Approve", false);  
                command.Parameters.AddWithValue("@Id", doctor_approvel_id);
                command.ExecuteNonQuery();

                string email = GetDoctorEmail(doctor_approvel_id);  
                SendRejectionEmail(email);  
            }
            return RedirectToPage("/Admin/UserData/Register_inquiry");
        }

        private string GetDoctorEmail(int doctor_approvel_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT email FROM Doctor_approvel WHERE doctor_approvel_id = @Id", connection);
                command.Parameters.AddWithValue("@Id", doctor_approvel_id);

                var result = command.ExecuteScalar(); 
                return result?.ToString();
            }
        }

        private async Task SendRejectionEmail(string email)
        {
            string emailBody = "Your account will be rejected. Thank you";

            var emailSent = await _emailService.SendEmailAsync(
                email,
                "Doctor Registration Verification",
                emailBody
            );
        }

        private async Task SendApprovalEmail(string email)
        {
            string emailBody = "Your account will be activated. Thank you";

            var emailSent = await _emailService.SendEmailAsync(
                email, 
                "Doctor Registration Verification",
                emailBody
            );
        }





    }
}
