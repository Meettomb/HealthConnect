using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HealthConnect.Models;
using Microsoft.Data.SqlClient;
using static HealthConnect.Pages.IndexModel;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HealthConnect.Pages.Settings
{
    public class Edit_personel_detailModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;


        [BindProperty]
        public int id { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Role { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Dob { get; set; }
        public bool? DoctorProfileComplete { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }


        [BindProperty]
        public User_Table User { get; set; }
        public List<User_Table> User_list = new List<User_Table>();

        public List<User_Table> User_TableIdList { get; set; } = new List<User_Table>();
        public List<DoctorCount> DoctorCounts { get; set; } = new List<DoctorCount>();


        [BindProperty]
        public Types_of_Doctor TypesOfDoctor { get; set; } = new Types_of_Doctor();

        public List<Types_of_Doctor> Types_of_doctor { get; set; } = new List<Types_of_Doctor>();

        [BindProperty]
        public Doctor_Specialitis SpecialitisOfDoctor { get; set; } = new Doctor_Specialitis();

        public List<Doctor_Specialitis> doctorSpecialitiesList = new List<Doctor_Specialitis>();

        public int TotalSpecialitiesCount { get; set; }
        public int TotalUsersCount { get; set; }
        public int TotalDoctorsCount { get; set; }
        public Edit_personel_detailModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("HealthConnect");
        }

        public IActionResult OnGet()
        {
            string roleInSession = HttpContext.Session.GetString("UserRole");
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId.HasValue)
            {
                OnGetLoginUserDetail();
                GetUserDetailsForUpdaeDetails(UserId.Value);
            }
            else
            {
                return RedirectToPage("/User/Sign_in");
            }

            return Page();
        }

        public void OnGetLoginUserDetail()
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
                            Country = reader["country"].ToString();
                            State = reader["state"].ToString();
                            City = reader["city"].ToString();
                            UserId = (int?)reader["id"];
                            DoctorProfileComplete = reader["doctor_profile_complete"] != DBNull.Value ? (bool?)(Convert.ToInt32(reader["doctor_profile_complete"]) == 1) : null;
                            Dob = reader["dob"].ToString();
                            Dob = reader["House_number_and_Street_name"].ToString();
                            Dob = reader["pincode"].ToString();


                        }
                    }
                    con.Close();
                }
            }
        }

        private void GetUserDetailsForUpdaeDetails(int UserId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
SELECT u.id, u.first_name, u.last_name, u.email, u.mobil_no, 
    u.dob, u.House_number_and_Street_name, u.country, u.city, 
    u.state, u.pincode, u.gender, u.role, u.password, 
    u.profile_pic, u.doctore_medical_license_photo, u.medical_registration_no, 
    u.state_medical_council, u.year_of_registration, u.doctore_experience, 
    u.hospital_or_clinic, u.doctor_qualifications, 
    t.type_of_doctor AS doctor_type, -- Fetching doctor type name
    u.languages_spoken, u.clinic_or_hospital_address, u.on_site_consultation_fee, 
    u.doctor_profile_complete, u.account_create_date, u.block, u.isactive, 
    u.mobail_verifie, u.auth_token, u.medicine_type, 
    u.currency_code, u.video_call_consultation_fee, 
    STRING_AGG(ds.doctor_specialitis, ', ') AS doctor_specialitis_list, 
    u.work_start_time, u.work_end_time, u.weekly_work_days, 
    u.max_time_per_appointments, u.break_between_two_appointments
FROM User_Table u
LEFT JOIN Types_of_Doctor t ON u.doctor_type = t.doctor_type_id -- Joining with Types_of_Doctor table
LEFT JOIN Doctor_Specialitis ds 
    ON ds.doctor_specialitis_id IN (SELECT value FROM STRING_SPLIT(u.doctor_specialitis, ',')) 
WHERE u.id = @UserId
GROUP BY u.id, u.first_name, u.last_name, u.email, u.mobil_no, 
    u.dob, u.House_number_and_Street_name, u.country, u.city, 
    u.state, u.pincode, u.gender, u.role, u.password, 
    u.profile_pic, u.doctore_medical_license_photo, u.medical_registration_no, 
    u.state_medical_council, u.year_of_registration, u.doctore_experience, 
    u.hospital_or_clinic, u.doctor_qualifications, t.type_of_doctor, 
    u.languages_spoken, u.clinic_or_hospital_address, u.on_site_consultation_fee, 
    u.doctor_profile_complete, u.account_create_date, u.block, u.isactive, 
    u.mobail_verifie, u.auth_token, u.medicine_type, 
    u.currency_code, u.video_call_consultation_fee, u.work_start_time, u.work_end_time, u.weekly_work_days, 
    u.max_time_per_appointments, u.break_between_two_appointments";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int currentYear = DateTime.Now.Year;
                            int doctorExperienceYear = reader.IsDBNull(19) ? 0 : int.Parse(reader.GetString(19));
                            int experienceInYears = doctorExperienceYear == 0 ? 0 : (currentYear - doctorExperienceYear) - 2;

                            var userData = new User_Table
                            {
                                id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                first_name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                last_name = reader.IsDBNull(2) ? null : reader.GetString(2),
                                email = reader.IsDBNull(3) ? null : reader.GetString(3),
                                mobil_no = reader.IsDBNull(4) ? null : reader.GetString(4),
                                dob = reader.IsDBNull(5) ? null : reader.GetString(5),
                                House_number_and_Street_name = reader.IsDBNull(6) ? null : reader.GetString(6),
                                country = reader.IsDBNull(7) ? null : reader.GetString(7),
                                city = reader.IsDBNull(8) ? null : reader.GetString(8),
                                state = reader.IsDBNull(9) ? null : reader.GetString(9),
                                pincode = reader.IsDBNull(10) ? null : reader.GetString(10),
                                gender = reader.IsDBNull(11) ? null : reader.GetString(11),
                                role = reader.IsDBNull(12) ? null : reader.GetString(12),
                                password = reader.IsDBNull(13) ? null : reader.GetString(13),
                                profile_pic = reader.IsDBNull(14) ? null : reader.GetString(14),
                                doctore_medical_license_photo = reader.IsDBNull(15) ? null : reader.GetString(15),
                                medical_registration_no = reader.IsDBNull(16) ? null : reader.GetString(16),
                                state_medical_council = reader.IsDBNull(17) ? null : reader.GetString(17),
                                year_of_registration = reader.IsDBNull(18) ? "0" : reader.GetString(18),
                                doctore_experience = experienceInYears.ToString(),
                                hospital_or_clinic = reader.IsDBNull(20) ? null : reader.GetString(20),
                                doctor_qualifications = reader.IsDBNull(21) ? null : reader.GetString(21),
                                doctor_type = reader.IsDBNull(22) ? null : reader.GetString(22),
                                languages_spoken = reader.IsDBNull(23) ? null : reader.GetString(23),
                                clinic_or_hospital_address = reader.IsDBNull(24) ? null : reader.GetString(24),
                                on_site_consultation_fee = reader.IsDBNull(25) ? null : reader.GetString(25),
                                doctor_profile_complete = reader.IsDBNull(26) ? false : reader.GetBoolean(26),
                                account_create_date = reader.IsDBNull(27) ? DateTime.MinValue : reader.GetDateTime(27),
                                block = reader.IsDBNull(28) ? false : reader.GetBoolean(28),
                                isactive = reader.IsDBNull(29) ? false : reader.GetBoolean(29),
                                mobail_verifie = reader.IsDBNull(30) ? false : reader.GetBoolean(30),
                                auth_token = reader.IsDBNull(31) ? null : reader.GetString(31),
                                medicine_type = reader.IsDBNull(32) ? null : reader.GetString(32),
                                currency_code = reader.IsDBNull(33) ? null : reader.GetString(33),
                                video_call_consultation_fee = reader.IsDBNull(34) ? null : reader.GetString(34),
                                doctor_specialitis = reader.IsDBNull(35) ? null : reader.GetString(35),
                                work_start_time = reader.IsDBNull(36) ? null : reader.GetString(36),
                                work_end_time = reader.IsDBNull(37) ? null : reader.GetString(37),
                                weekly_work_days = reader.IsDBNull(38) ? new List<string>() : reader.GetString(38).Split(',').ToList(),
                                max_time_per_appointments = reader.IsDBNull(39) ? null : reader.GetString(39),
                                break_between_two_appointments = reader.IsDBNull(40) ? null : reader.GetString(40)
                            };

                            User_list.Add(userData);
                        }
                    }
                }
            }
        }





        public async Task<IActionResult> OnPostUpdateProfilePicAsync()
        {
            var profilePic = Request.Form.Files["profile_pic"];
            var idValue = Request.Form["id"];

            if (profilePic == null || string.IsNullOrEmpty(idValue))
            {
                TempData["ErrorMessage"] = "Profile picture and ID are required.";
                return Page();
            }

            int userId = Convert.ToInt32(idValue);

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documant");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var profilePicPath = Path.Combine(folderPath, profilePic.FileName);

            using (var stream = new FileStream(profilePicPath, FileMode.Create))
            {
                await profilePic.CopyToAsync(stream);
            }

            User_Table updatedUser = new User_Table
            {
                id = userId,
                profile_pic = profilePic.FileName
            };

            bool isProfilePicUpdated = await UpdateProfilePicAsync(updatedUser);

            if (isProfilePicUpdated)
            {
                TempData["SuccessMessage"] = "Profile picture updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile picture.";
            }

            return RedirectToPage("/Account/Settings/Edit_personel_detail");
        }
        private async Task<bool> UpdateProfilePicAsync(User_Table updatedUser)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE User_Table SET profile_pic = @profilePic WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@profilePic", updatedUser.profile_pic ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", updatedUser.id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }



        public async Task<IActionResult> OnPostAsync()
        {
            var idValue = Request.Form["id"];
            if (string.IsNullOrEmpty(idValue))
            {
                return Page();
            }

            int userId = Convert.ToInt32(idValue);

            User_Table user = new User_Table
            {
                id = userId,
                first_name = Request.Form["first_name"],
                last_name = Request.Form["last_name"],
                dob = Request.Form["dob"],
                state = Request.Form["state"],
                city = Request.Form["city"],
                House_number_and_Street_name = Request.Form["House_number_and_Street_name"],
                pincode = Request.Form["pincode"]
            };

            bool isBasicDetailUpdated = await UpdateBasicDetailAsync(user);
            if (isBasicDetailUpdated)
            {
                TempData["SuccessMessage"] = "Basic detail updated successfully.";
            }

            return RedirectToPage("/Account/Settings/Edit_personel_detail");
        }
        private async Task<bool> UpdateBasicDetailAsync(User_Table user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE User_Table 
                         SET first_name = @first_name, 
                             last_name = @last_name, 
                             dob = @dob, 
                             state = @state, 
                             city = @city, 
                             House_number_and_Street_name = @House_number_and_Street_name, 
                             pincode = @pincode 
                         WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@first_name", user.first_name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@last_name", user.last_name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@dob", user.dob ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@state", user.state ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@city", user.city ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@House_number_and_Street_name", user.House_number_and_Street_name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@pincode", user.pincode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", user.id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }



        public async Task<IActionResult> OnPostRemoveProfilePicAsync()
        {
            var idValue = Request.Form["id"];

            if (string.IsNullOrEmpty(idValue))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return Page();
            }

            int userId = Convert.ToInt32(idValue);
            User_Table removeUser = new User_Table { id = userId };

            bool isPicRemoved = await RemovePicAsync(removeUser);

            if (isPicRemoved)
            {
                TempData["SuccessMessage"] = "Profile picture removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to remove profile picture.";
            }

            return RedirectToPage("/Account/Settings/Edit_personel_detail");
        }
        private async Task<bool> RemovePicAsync(User_Table removeUserPic)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE User_Table SET profile_pic = NULL WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", removeUserPic.id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }




        public async Task<IActionResult> OnPostUpdateDoctorInfoAsync()
        {
            var idValue = Request.Form["id"];
            if (string.IsNullOrEmpty(idValue))
            {
                return Page();
            }

            int userId = Convert.ToInt32(idValue);

            User_Table user = new User_Table
            {
                id = userId,
                hospital_or_clinic = Request.Form["hospital_or_clinic"],
                clinic_or_hospital_address = Request.Form["clinic_or_hospital_address"],
                on_site_consultation_fee = Request.Form["on_site_consultation_fee"],
                video_call_consultation_fee = Request.Form["video_call_consultation_fee"],
            };

            bool isUpdateDoctorInfo = await UpdateDoctorInfo(user);
            if (isUpdateDoctorInfo)
            {
                TempData["SuccessMessage"] = "Doctore detail updated successfully.";
                SuccessMessage = "Doctore detail updated successfully.";
            }

            return RedirectToPage("/Account/Overview");
        }
        private async Task<bool> UpdateDoctorInfo(User_Table UpdateDoctorInfo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"UPDATE User_Table 
                         SET hospital_or_clinic = @hospital_or_clinic, 
                             clinic_or_hospital_address = @clinic_or_hospital_address, 
                             on_site_consultation_fee = @on_site_consultation_fee, 
                             video_call_consultation_fee = @video_call_consultation_fee
                         WHERE id = @id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@hospital_or_clinic", UpdateDoctorInfo.hospital_or_clinic ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@clinic_or_hospital_address", UpdateDoctorInfo.clinic_or_hospital_address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@on_site_consultation_fee", UpdateDoctorInfo.on_site_consultation_fee ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@video_call_consultation_fee", UpdateDoctorInfo.video_call_consultation_fee ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@id", UpdateDoctorInfo.id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


    }




}

