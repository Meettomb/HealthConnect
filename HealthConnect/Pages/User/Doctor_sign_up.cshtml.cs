using HealthConnect.Models;
using HealthConnect.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthConnect.Pages.User
{
    public class Doctor_sign_upModel : PageModel
    {
        private readonly ILogger<Doctor_sign_upModel> _logger;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly string _connectionString;

        public Doctor_sign_upModel(IEmailService emailService, IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _connectionString = configuration.GetConnectionString("HealthConnect");

        }

        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

        [BindProperty]
        public Doctor_approvel doctor_Approvel { get; set; }

        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string DOB { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public string Gender { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string Account_create_date { get; set; }
        public string CurrencyCode { get; set; }

        public int? UserId { get; set; }

        public string SessionOtp { get; set; }

        public IActionResult OnGet()
        {
            FirstName = HttpContext.Session.GetString("FirstName");
            LastName = HttpContext.Session.GetString("LastName");
            Email = HttpContext.Session.GetString("Email");
            MobileNo = HttpContext.Session.GetString("MobileNo");
            DOB = HttpContext.Session.GetString("DOB");
            Address = HttpContext.Session.GetString("Address");
            Country = HttpContext.Session.GetString("Country");
            City = HttpContext.Session.GetString("City");
            State = HttpContext.Session.GetString("State");
            Pincode = HttpContext.Session.GetString("Pincode");
            Gender = HttpContext.Session.GetString("Gender");
            Role = HttpContext.Session.GetString("Role");
            Password = HttpContext.Session.GetString("Password");
            Account_create_date = HttpContext.Session.GetString("Account_create_date");
            CurrencyCode = HttpContext.Session.GetString("CurrencyCode");

            SessionOtp = HttpContext.Session.GetString("OTP");

            UserId = HttpContext.Session.GetInt32("Id");
            string roleInSession = HttpContext.Session.GetString("UserRole");



            Types_of_doctor = new List<Types_of_Doctor>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Types_of_Doctor";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Types_of_doctor.Add(new Types_of_Doctor
                            {
                                doctor_type_id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                type_of_doctor = reader.GetString(1),
                            });
                        }
                    }
                }
            }

            // Initialize doctorSpecialitiesList
            doctorSpecialitiesList = new List<Doctor_Specialitis>();



          
            if (UserId.HasValue && !string.IsNullOrEmpty(roleInSession))
            {
                if (roleInSession == "Admin")
                {
                    return RedirectToPage("/Admin/Admin_index");
                }
                else if (roleInSession == "User" || roleInSession == "Doctor")
                {
                    return RedirectToPage("/index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid role in session.");
                }
            }

            string deviceId = Request.Cookies["deviceUniqueId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT id, first_name, last_name, email, role, isactive 
                         FROM User_Table 
                         WHERE ',' + auth_token + ',' LIKE '%,' + @DeviceId + ',%'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["id"]);
                        string firstName = reader["first_name"].ToString();
                        string lastName = reader["last_name"].ToString();
                        string email = reader["email"].ToString();
                        string role = reader["role"].ToString();
                        bool isActive = Convert.ToBoolean(reader["isactive"]);

                        if (isActive)
                        {
                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("FirstName", firstName);
                            HttpContext.Session.SetString("LastName", lastName);
                            HttpContext.Session.SetString("Email", email);
                            HttpContext.Session.SetString("UserRole", role);

                            // Redirect based on role
                            if (role == "Admin")
                            {
                                return RedirectToPage("/Admin/Admin_index");
                            }
                            else if (role == "User" || role == "Doctor")
                            {
                                return RedirectToPage("/index");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid role.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Account is not active.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Device not recognized or account not found.");
                    }
                }
            }

            return Page();
        }

        public JsonResult OnGetFetchSpecialities(int doctorTypeId)
        {
            var doctorSpecialities = new List<Doctor_Specialitis>();

            string query = @"
    SELECT 
        DS.doctor_specialitis_id,
        DS.doctor_type_id,
        DS.doctor_specialitis
    FROM 
        Doctor_Specialitis DS
    WHERE 
        DS.doctor_type_id = @doctorTypeId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@doctorTypeId", doctorTypeId);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doctorSpecialities.Add(new Doctor_Specialitis
                            {
                                doctor_specialitis_id = reader.GetInt32(0),
                                doctor_type_id = reader.GetInt32(1),
                                doctor_specialitis = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return new JsonResult(doctorSpecialities);
        }



        public async Task<IActionResult> OnPostAsync()
        {
            if (Request.Form.Files.Count == 0)
            {
                ErrorMessage = "Please upload both profile picture and medical license photo.";
                return Page();
            }

            var profilePic = Request.Form.Files["profile_pic"];
            var licensePic = Request.Form.Files["doctore_medical_license_photo"];

            if (profilePic == null || licensePic == null)
            {
                ErrorMessage = "Both profile picture and medical license photo are required.";
                return Page();
            }

            var profilePicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documant", profilePic.FileName);
            var licensePicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documant", licensePic.FileName);

            using (var stream = new FileStream(profilePicPath, FileMode.Create))
            {
                await profilePic.CopyToAsync(stream);
            }

            using (var stream = new FileStream(licensePicPath, FileMode.Create))
            {
                await licensePic.CopyToAsync(stream);
            }

            doctor_Approvel.profile_pic = profilePic.FileName;
            doctor_Approvel.doctore_medical_license_photo = licensePic.FileName;

            doctor_Approvel.first_name = HttpContext.Session.GetString("FirstName");
            doctor_Approvel.last_name = HttpContext.Session.GetString("LastName");
            doctor_Approvel.email = HttpContext.Session.GetString("Email");
            doctor_Approvel.mobil_no = HttpContext.Session.GetString("MobileNo");
            doctor_Approvel.dob = HttpContext.Session.GetString("DOB");
            doctor_Approvel.House_number_and_Street_name = HttpContext.Session.GetString("Address");
            doctor_Approvel.country = HttpContext.Session.GetString("Country");
            doctor_Approvel.city = HttpContext.Session.GetString("City");
            doctor_Approvel.state = HttpContext.Session.GetString("State");
            doctor_Approvel.pincode = HttpContext.Session.GetString("Pincode");
            doctor_Approvel.gender = HttpContext.Session.GetString("Gender");
            doctor_Approvel.role = HttpContext.Session.GetString("Role");
            doctor_Approvel.currency_code = HttpContext.Session.GetString("CurrencyCode");
            var hashedPassword = new PasswordHasher<Doctor_approvel>().HashPassword(doctor_Approvel, HttpContext.Session.GetString("Password"));
            doctor_Approvel.password = hashedPassword;

            doctor_Approvel.medical_registration_no = Request.Form["medical_registration_no"];
            doctor_Approvel.state_medical_council = Request.Form["state_medical_council"];
            doctor_Approvel.year_of_registration = Request.Form["year_of_registration"];
            doctor_Approvel.doctore_experience = Request.Form["doctore_experience"];
            doctor_Approvel.hospital_or_clinic = Request.Form["hospital_or_clinic"];
            doctor_Approvel.doctor_qualifications = Request.Form["doctor_qualifications"];
            doctor_Approvel.doctor_type = string.Join(", ", Request.Form["doctor_type"]);
            doctor_Approvel.languages_spoken = string.Join(", ", Request.Form["languages_spoken"]);
            doctor_Approvel.clinic_or_hospital_address = Request.Form["clinic_or_hospital_address"];
            doctor_Approvel.on_site_consultation_fee = Request.Form["on_site_consultation_fee"];
            doctor_Approvel.medicine_type = Request.Form["medicine_type"];
            doctor_Approvel.account_create_date = DateTime.Now;
            doctor_Approvel.video_call_consultation_fee = Request.Form["video_call_consultation_fee"];
            doctor_Approvel.doctor_specialitis = Request.Form["doctor_specialitis"];

            bool isSaved = await SaveDoctorApprovalToDatabase(doctor_Approvel);
            if (!isSaved)
            {
                ErrorMessage = "An error occurred while saving your information.";
                return Page();
            }

            string emailBody = "Your account will be activated after verifying your details. This process may take a few days.";
            var emailSent = await _emailService.SendEmailAsync(
                doctor_Approvel.email,
                "Doctor Registration Verification",
                emailBody
            );

            if (emailSent)
            {
                SuccessMessage = "Your registration is successful. Please check your email for verification.";
                HttpContext.Session.Clear();
            }
            else
            {
                ErrorMessage = "An error occurred while sending the email.";
            }

            return RedirectToPage("/Index");
        }

        private async Task<bool> SaveDoctorApprovalToDatabase(Doctor_approvel doctor_Approvel)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO Doctor_approvel 
                    (first_name, last_name, email, mobil_no, dob, House_number_and_Street_name, country, city, state, pincode, gender, role, password, 
                     medical_registration_no, state_medical_council, year_of_registration, doctore_experience, hospital_or_clinic, doctor_qualifications, 
                     doctor_type, languages_spoken, clinic_or_hospital_address, on_site_consultation_fee, account_create_date, profile_pic, doctore_medical_license_photo, 
                     isactive, medicine_type, currency_code, video_call_consultation_fee, doctor_specialitis)
                  VALUES 
                    (@FirstName, @LastName, @Email, @MobilNo, @DOB, @HouseNoStreetName, @Country, @City, @State, @Pincode, @Gender, @Role, @Password, 
                     @MedicalRegistrationNo, @StateMedicalCouncil, @YearOfRegistration, @DoctorExperience, @HospitalOrClinic, @DoctorQualifications, 
                     @DoctorType, @LanguagesSpoken, @ClinicOrHospitalAddress, @On_site_consultation_fee, @AccountCreateDate, @ProfilePic, @MedicalLicensePhoto, 
                     @IsActive, @Medicine_type, @Currency_code, @Video_call_consultation_fee, @Doctor_specialitis)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", doctor_Approvel.first_name);
                    command.Parameters.AddWithValue("@LastName", doctor_Approvel.last_name);
                    command.Parameters.AddWithValue("@Email", doctor_Approvel.email);
                    command.Parameters.AddWithValue("@MobilNo", doctor_Approvel.mobil_no);
                    command.Parameters.AddWithValue("@DOB", doctor_Approvel.dob);
                    command.Parameters.AddWithValue("@HouseNoStreetName", doctor_Approvel.House_number_and_Street_name);
                    command.Parameters.AddWithValue("@Country", doctor_Approvel.country);
                    command.Parameters.AddWithValue("@City", doctor_Approvel.city);
                    command.Parameters.AddWithValue("@State", doctor_Approvel.state);
                    command.Parameters.AddWithValue("@Pincode", doctor_Approvel.pincode);
                    command.Parameters.AddWithValue("@Gender", doctor_Approvel.gender);
                    command.Parameters.AddWithValue("@Role", doctor_Approvel.role);
                    command.Parameters.AddWithValue("@Password", doctor_Approvel.password);
                    command.Parameters.AddWithValue("@MedicalRegistrationNo", doctor_Approvel.medical_registration_no);
                    command.Parameters.AddWithValue("@StateMedicalCouncil", doctor_Approvel.state_medical_council);
                    command.Parameters.AddWithValue("@YearOfRegistration", doctor_Approvel.year_of_registration);
                    command.Parameters.AddWithValue("@DoctorExperience", doctor_Approvel.doctore_experience);
                    command.Parameters.AddWithValue("@HospitalOrClinic", doctor_Approvel.hospital_or_clinic);
                    command.Parameters.AddWithValue("@DoctorQualifications", doctor_Approvel.doctor_qualifications);
                    command.Parameters.AddWithValue("@DoctorType", doctor_Approvel.doctor_type);
                    command.Parameters.AddWithValue("@LanguagesSpoken", doctor_Approvel.languages_spoken);
                    command.Parameters.AddWithValue("@ClinicOrHospitalAddress", doctor_Approvel.clinic_or_hospital_address);
                    command.Parameters.AddWithValue("@On_site_consultation_fee", doctor_Approvel.on_site_consultation_fee);
                    command.Parameters.AddWithValue("@AccountCreateDate", doctor_Approvel.account_create_date);
                    command.Parameters.AddWithValue("@ProfilePic", doctor_Approvel.profile_pic);
                    command.Parameters.AddWithValue("@MedicalLicensePhoto", doctor_Approvel.doctore_medical_license_photo);
                    command.Parameters.AddWithValue("@Medicine_type", doctor_Approvel.medicine_type);
                    command.Parameters.AddWithValue("@IsActive", false);
                    command.Parameters.AddWithValue("@Currency_code", doctor_Approvel.currency_code);
                    command.Parameters.AddWithValue("@Video_call_consultation_fee", doctor_Approvel.video_call_consultation_fee);
                    command.Parameters.AddWithValue("@Doctor_specialitis", doctor_Approvel.doctor_specialitis);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


    }



}


