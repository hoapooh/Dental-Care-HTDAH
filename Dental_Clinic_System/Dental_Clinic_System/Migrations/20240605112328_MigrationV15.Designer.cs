﻿// <auto-generated />
using System;
using Dental_Clinic_System.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Dental_Clinic_System.Migrations
{
    [DbContext(typeof(DentalClinicDbContext))]
    [Migration("20240605112328_MigrationV15")]
    partial class MigrationV15
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("AccountStatus")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("AccountStatus");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Address");

                    b.Property<DateOnly?>("DateOfBirth")
                        .HasColumnType("DATE")
                        .HasColumnName("DateOfBirth");

                    b.Property<string>("District")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("District");

                    b.Property<string>("Email")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("Email");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("FirstName");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(6)")
                        .HasColumnName("Gender");

                    b.Property<string>("Image")
                        .HasColumnType("varchar(256)")
                        .HasColumnName("Image");

                    b.Property<bool?>("IsLinked")
                        .HasColumnType("bit")
                        .HasColumnName("IsLinked");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("LastName");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("varchar(30)")
                        .HasColumnName("Password");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("char(11)")
                        .HasColumnName("PhoneNumber")
                        .IsFixedLength();

                    b.Property<string>("Province")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Province");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(9)")
                        .HasColumnName("Role");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(30)")
                        .HasColumnName("Username");

                    b.Property<string>("Ward")
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Ward");

                    b.HasKey("ID")
                        .HasName("PK_Account");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasFilter("[Email] IS NOT NULL");

                    b.HasIndex("PhoneNumber")
                        .IsUnique()
                        .HasFilter("[PhoneNumber] IS NOT NULL");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Account", t =>
                        {
                            t.HasCheckConstraint("CK_Valid_Gender", "[Gender] = 'Male' OR [Gender] = 'Female' OR [Gender] = N'Nam' OR [Gender] = N'Nữ'");

                            t.HasCheckConstraint("CK_Valid_Role", "[Role] = 'Admin' OR [Role] = 'PatientRecord' OR [Role] = 'Dentist' OR [Role] = 'Manager' OR [Role] = N'Bệnh Nhân' OR [Role] = N'Nha Sĩ' OR [Role] = N'Quản Lý'");

                            t.HasCheckConstraint("CK_Valid_Status_Account", "[AccountStatus] = 'Active' OR [AccountStatus] = 'Banned' OR [AccountStatus] = N'Not Active' OR [AccountStatus] = N'Hoạt Động' OR [AccountStatus] = N'Bị Khóa' OR [AccountStatus] = N'Chưa Kích Hoạt'");
                        });
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Appointment", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("AppointmentStatus")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("AppointmentStatus");

                    b.Property<int>("PatientID")
                        .HasColumnType("int");

                    b.Property<int>("PatientRecordID")
                        .HasColumnType("int")
                        .HasColumnName("PatientRecordID");

                    b.Property<int>("ScheduleID")
                        .HasColumnType("int")
                        .HasColumnName("ScheduleID");

                    b.Property<int>("SpecialtyID")
                        .HasColumnType("int")
                        .HasColumnName("SpecialtyID");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("money")
                        .HasColumnName("TotalPrice");

                    b.HasKey("ID")
                        .HasName("PK_Appointment");

                    b.HasIndex("PatientID");

                    b.HasIndex("ScheduleID")
                        .IsUnique();

                    b.HasIndex("SpecialtyID");

                    b.ToTable("Appointment", t =>
                        {
                            t.HasCheckConstraint("CK_Valid_Status_Appointment", "[AppointmentStatus] = 'Pending' OR [AppointmentStatus] = 'Canceled' OR [AppointmentStatus] = 'Approved' OR [AppointmentStatus] = 'Completed' OR [AppointmentStatus] = N'Chờ Xác Nhận' OR [AppointmentStatus] = N'Đã Hủy' OR [AppointmentStatus] = N'Đã Chấp Nhận' OR [AppointmentStatus] = N'Đã Khám'");
                        });
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Clinic", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Address");

                    b.Property<string>("Basis")
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Basis");

                    b.Property<string>("Description")
                        .HasColumnType("ntext")
                        .HasColumnName("Description");

                    b.Property<string>("District")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("District");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("varchar(256)")
                        .HasColumnName("Image");

                    b.Property<int>("ManagerID")
                        .HasColumnType("int")
                        .HasColumnName("ManagerID");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Province");

                    b.Property<string>("Ward")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Ward");

                    b.HasKey("ID")
                        .HasName("PK_Clinic");

                    b.HasIndex("ManagerID")
                        .IsUnique();

                    b.ToTable("Clinic");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Degree", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.HasKey("ID")
                        .HasName("PK_Degree");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Degree");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Dentist", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<int>("AccountID")
                        .HasColumnType("int")
                        .HasColumnName("AccountID");

                    b.Property<int>("ClinicID")
                        .HasColumnType("int")
                        .HasColumnName("ClinicID");

                    b.Property<int>("DegreeID")
                        .HasColumnType("int")
                        .HasColumnName("DegreeID");

                    b.Property<string>("Description")
                        .HasColumnType("ntext");

                    b.HasKey("ID")
                        .HasName("PK_Dentist");

                    b.HasIndex("AccountID")
                        .IsUnique();

                    b.HasIndex("ClinicID");

                    b.HasIndex("DegreeID");

                    b.ToTable("Dentist");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.DentistSpecialty", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<int>("DentistID")
                        .HasColumnType("int")
                        .HasColumnName("DentistID");

                    b.Property<int>("SpecialtyID")
                        .HasColumnType("int")
                        .HasColumnName("SpecialtyID");

                    b.HasKey("ID")
                        .HasName("PK_DentistSpecialty");

                    b.HasIndex("DentistID");

                    b.HasIndex("SpecialtyID");

                    b.ToTable("Dentist_Specialty");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.PatientRecord", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<int>("AccountID")
                        .HasColumnType("int")
                        .HasColumnName("AccountID");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Address");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date")
                        .HasColumnName("DateOfBirth");

                    b.Property<string>("District")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("District");

                    b.Property<string>("EmailReceiver")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("EmailReceiver");

                    b.Property<string>("FMName")
                        .HasColumnType("nvarchar(75)")
                        .HasColumnName("FMName");

                    b.Property<string>("FMPhoneNumber")
                        .HasColumnType("varchar(11)")
                        .HasColumnName("FMPhoneNumber");

                    b.Property<string>("FMRelationship")
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("FMRelationship");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(75)")
                        .HasColumnName("FullName");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(6)")
                        .HasColumnName("Gender");

                    b.Property<string>("IdentityNumber")
                        .HasColumnType("varchar(12)")
                        .HasColumnName("IdentityNumber");

                    b.Property<string>("Job")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Job");

                    b.Property<string>("MemberCard")
                        .HasColumnType("nvarchar(15)")
                        .HasColumnName("MemberCard")
                        .IsFixedLength();

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("varchar(11)")
                        .HasColumnName("PhoneNumber");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Province");

                    b.Property<string>("Ward")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Ward");

                    b.HasKey("ID")
                        .HasName("PK_Patient");

                    b.HasIndex("AccountID");

                    b.HasIndex("MemberCard")
                        .IsUnique()
                        .HasFilter("[MemberCard] IS NOT NULL");

                    b.ToTable("PatientRecord");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Review", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("nvarchar(2000)")
                        .HasColumnName("Comment");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("DATE")
                        .HasColumnName("Date");

                    b.Property<int>("DentistID")
                        .HasColumnType("int")
                        .HasColumnName("DentistID");

                    b.Property<int>("PatientID")
                        .HasColumnType("int")
                        .HasColumnName("PatientID");

                    b.HasKey("ID")
                        .HasName("PK_Review");

                    b.HasIndex("DentistID");

                    b.HasIndex("PatientID");

                    b.ToTable("Review");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Schedule", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<DateOnly>("Date")
                        .HasColumnType("DATE")
                        .HasColumnName("Date");

                    b.Property<int>("DentistID")
                        .HasColumnType("int")
                        .HasColumnName("DentistID");

                    b.Property<string>("ScheduleStatus")
                        .IsRequired()
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("ScheduleStatus");

                    b.Property<int>("TimeSlotID")
                        .HasColumnType("int")
                        .HasColumnName("TimeSlotID");

                    b.HasKey("ID")
                        .HasName("PK_Schedule");

                    b.HasIndex("DentistID");

                    b.HasIndex("TimeSlotID");

                    b.ToTable("Schedule", t =>
                        {
                            t.HasCheckConstraint("CK_Valid_Schedule_Status", "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống'");
                        });
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Service", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<int>("ClinicID")
                        .HasColumnType("int")
                        .HasColumnName("ClinicID");

                    b.Property<string>("Description")
                        .HasColumnType("ntext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("Name");

                    b.Property<string>("Price")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("Price");

                    b.Property<int>("SpecialtyID")
                        .HasColumnType("int")
                        .HasColumnName("SpecialtyID");

                    b.HasKey("ID")
                        .HasName("PK_Service");

                    b.HasIndex("ClinicID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("SpecialtyID");

                    b.ToTable("Service");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Specialty", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<string>("Description")
                        .HasColumnType("ntext")
                        .HasColumnName("Description");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("varchar(256)")
                        .HasColumnName("Image");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("Name");

                    b.HasKey("ID")
                        .HasName("PK_Specialty");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Specialty");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.TimeSlot", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time(7)")
                        .HasColumnName("EndTime");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time(7)")
                        .HasColumnName("StartTime");

                    b.HasKey("ID")
                        .HasName("PK_TimeSlot");

                    b.ToTable("TimeSlot");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Transaction", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("ID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ID"));

                    b.Property<int>("AppointmentID")
                        .HasColumnType("int")
                        .HasColumnName("AppointmentID");

                    b.Property<string>("BankAccountNumber")
                        .IsRequired()
                        .HasColumnType("varchar(20)")
                        .HasColumnName("BankAccountNumber")
                        .IsFixedLength();

                    b.Property<string>("BankName")
                        .IsRequired()
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("BankName");

                    b.Property<DateTime>("Date")
                        .HasColumnType("DATETIME")
                        .HasColumnName("Date");

                    b.HasKey("ID")
                        .HasName("PK_Transaction");

                    b.HasIndex("AppointmentID");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Appointment", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.PatientRecord", "PatientRecords")
                        .WithMany("Appointments")
                        .HasForeignKey("PatientID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK__Appointment__PatientRecord");

                    b.HasOne("Dental_Clinic_System.Models.Data.Schedule", "Schedule")
                        .WithOne("Appointments")
                        .HasForeignKey("Dental_Clinic_System.Models.Data.Appointment", "ScheduleID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("FK__Appointment__Schedule");

                    b.HasOne("Dental_Clinic_System.Models.Data.Specialty", "Specialty")
                        .WithMany("Appointments")
                        .HasForeignKey("SpecialtyID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK__Appointment__Specialty");

                    b.Navigation("PatientRecords");

                    b.Navigation("Schedule");

                    b.Navigation("Specialty");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Clinic", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Account", "Manager")
                        .WithOne("Clinics")
                        .HasForeignKey("Dental_Clinic_System.Models.Data.Clinic", "ManagerID")
                        .HasConstraintName("FK__Clinic__Manager");

                    b.Navigation("Manager");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Dentist", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Account", "Account")
                        .WithOne("Dentists")
                        .HasForeignKey("Dental_Clinic_System.Models.Data.Dentist", "AccountID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK__Dentist__Account");

                    b.HasOne("Dental_Clinic_System.Models.Data.Clinic", "Clinic")
                        .WithMany("Dentists")
                        .HasForeignKey("ClinicID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__Dentist__Clinic");

                    b.HasOne("Dental_Clinic_System.Models.Data.Degree", "Degree")
                        .WithMany("Dentists")
                        .HasForeignKey("DegreeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__Dentist__Degree");

                    b.Navigation("Account");

                    b.Navigation("Clinic");

                    b.Navigation("Degree");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.DentistSpecialty", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Dentist", "Dentist")
                        .WithMany("DentistSpecialties")
                        .HasForeignKey("DentistID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__DentistSpecialty__Dentist");

                    b.HasOne("Dental_Clinic_System.Models.Data.Specialty", "Specialty")
                        .WithMany("DentistSpecialties")
                        .HasForeignKey("SpecialtyID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__DentistSpecialty__Specialty");

                    b.Navigation("Dentist");

                    b.Navigation("Specialty");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.PatientRecord", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Account", "Account")
                        .WithMany("PatientRecords")
                        .HasForeignKey("AccountID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK__Patient__Account");

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Review", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Dentist", "Dentist")
                        .WithMany("Reviews")
                        .HasForeignKey("DentistID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__Review__Dentist");

                    b.HasOne("Dental_Clinic_System.Models.Data.Account", "Patient")
                        .WithMany("Reviews")
                        .HasForeignKey("PatientID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__Review__Patient");

                    b.Navigation("Dentist");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Schedule", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Dentist", "Dentist")
                        .WithMany("Schedules")
                        .HasForeignKey("DentistID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK__Schedule__Dentis");

                    b.HasOne("Dental_Clinic_System.Models.Data.TimeSlot", "TimeSlot")
                        .WithMany("Schedules")
                        .HasForeignKey("TimeSlotID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK__Schedule__TimeSlot");

                    b.Navigation("Dentist");

                    b.Navigation("TimeSlot");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Service", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Clinic", "Clinic")
                        .WithMany("Services")
                        .HasForeignKey("ClinicID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK__Service__Clinic");

                    b.HasOne("Dental_Clinic_System.Models.Data.Specialty", "Specialty")
                        .WithMany("Services")
                        .HasForeignKey("SpecialtyID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK__Service__Special");

                    b.Navigation("Clinic");

                    b.Navigation("Specialty");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Transaction", b =>
                {
                    b.HasOne("Dental_Clinic_System.Models.Data.Appointment", "Appointment")
                        .WithMany("Transactions")
                        .HasForeignKey("AppointmentID")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired()
                        .HasConstraintName("FK__Transacti__Appointment");

                    b.Navigation("Appointment");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Account", b =>
                {
                    b.Navigation("Clinics");

                    b.Navigation("Dentists");

                    b.Navigation("PatientRecords");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Appointment", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Clinic", b =>
                {
                    b.Navigation("Dentists");

                    b.Navigation("Services");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Degree", b =>
                {
                    b.Navigation("Dentists");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Dentist", b =>
                {
                    b.Navigation("DentistSpecialties");

                    b.Navigation("Reviews");

                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.PatientRecord", b =>
                {
                    b.Navigation("Appointments");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Schedule", b =>
                {
                    b.Navigation("Appointments")
                        .IsRequired();
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.Specialty", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("DentistSpecialties");

                    b.Navigation("Services");
                });

            modelBuilder.Entity("Dental_Clinic_System.Models.Data.TimeSlot", b =>
                {
                    b.Navigation("Schedules");
                });
#pragma warning restore 612, 618
        }
    }
}
