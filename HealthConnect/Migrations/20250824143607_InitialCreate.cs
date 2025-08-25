using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthConnect.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doctor_approvel",
                columns: table => new
                {
                    doctor_approvel_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mobil_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dob = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    House_number_and_Street_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pincode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    profile_pic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    doctore_medical_license_photo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    medical_registration_no = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state_medical_council = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    year_of_registration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    doctore_experience = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    hospital_or_clinic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    doctor_qualifications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    doctor_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    languages_spoken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    clinic_or_hospital_address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    on_site_consultation_fee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    account_approve = table.Column<bool>(type: "bit", nullable: true),
                    account_create_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isactive = table.Column<bool>(type: "bit", nullable: false),
                    medicine_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    currency_code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    video_call_consultation_fee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    doctor_specialitis = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor_approvel", x => x.doctor_approvel_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doctor_approvel");
        }
    }
}
