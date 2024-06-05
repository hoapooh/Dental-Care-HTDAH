﻿using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Dental_Clinic_System.Models.Data
{
	public partial class DentalClinicDbContext : DbContext
	{
		public DentalClinicDbContext() { }

		public DentalClinicDbContext(DbContextOptions<DentalClinicDbContext> options) : base(options) { }

		#region DbSet
		public virtual DbSet<Account> Accounts { get; set; }
		public virtual DbSet<Appointment> Appointments { get; set; }
		public virtual DbSet<Clinic> Clinics { get; set; }
		public virtual DbSet<Degree> Degrees { get; set; }
		public virtual DbSet<Dentist> Dentists { get; set; }
		public virtual DbSet<DentistSpecialty> DentistSpecialties { get; set; }
		public virtual DbSet<PatientRecord> PatientRecords { get; set; }
		public virtual DbSet<Review> Reviews { get; set; }
		public virtual DbSet<Schedule> Schedules { get; set; }
		public virtual DbSet<Service> Services { get; set; }
		public virtual DbSet<Specialty> Specialties { get; set; }
		public virtual DbSet<TimeSlot> TimeSlots { get; set; }
		public virtual DbSet<Transaction> Transactions { get; set; }
		#endregion

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Name=DBConnection");

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Account>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Account");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasIndex(e => e.Username).IsUnique();

				entity.Property(e => e.PhoneNumber).IsFixedLength();

				entity.HasIndex(e => e.PhoneNumber).IsUnique();

				entity.HasIndex(e => e.Email).IsUnique();

				entity.Property(e => e.Role).IsRequired();

				entity.Property(e => e.AccountStatus).IsRequired();


				entity.HasCheckConstraint("CK_Valid_Role", "[Role] = 'Admin' OR [Role] = 'PatientRecord' OR [Role] = 'Dentist' OR [Role] = 'Manager' OR [Role] = N'Bệnh Nhân' OR [Role] = N'Nha Sĩ' OR [Role] = N'Quản Lý'");

				entity.HasCheckConstraint("CK_Valid_Gender", "[Gender] = 'Male' OR [Gender] = 'Female' OR [Gender] = N'Nam' OR [Gender] = N'Nữ'");

				entity.HasCheckConstraint("CK_Valid_Status_Account", "[AccountStatus] = 'Active' OR [AccountStatus] = 'Banned' OR [AccountStatus] = N'Not Active' OR [AccountStatus] = N'Hoạt Động' OR [AccountStatus] = N'Bị Khóa' OR [AccountStatus] = N'Chưa Kích Hoạt'");
			});

			modelBuilder.Entity<Appointment>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Appointment");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasCheckConstraint("CK_Valid_Status_Appointment", "[AppointmentStatus] = 'Pending' OR [AppointmentStatus] = 'Canceled' OR [AppointmentStatus] = 'Approved' OR [AppointmentStatus] = 'Completed' OR [AppointmentStatus] = N'Chờ Xác Nhận' OR [AppointmentStatus] = N'Đã Hủy' OR [AppointmentStatus] = N'Đã Chấp Nhận' OR [AppointmentStatus] = N'Đã Khám'");

				entity.HasOne(d => d.PatientRecords).WithMany(p => p.Appointments).IsRequired(false).HasConstraintName("FK__Appointment__PatientRecord").OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(d => d.Schedule).WithOne(p => p.Appointments).IsRequired(true).HasConstraintName("FK__Appointment__Schedule").OnDelete(DeleteBehavior.NoAction);

				entity.HasOne(d => d.Specialty).WithMany(p => p.Appointments).HasConstraintName("FK__Appointment__Specialty");
			});

			modelBuilder.Entity<Clinic>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Clinic");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				//entity.ToTable("Clinic", tb => tb.HasTrigger("trg_Clinic_Manager_Insert"));

				entity.HasOne(d => d.Manager).WithOne(p => p.Clinics).IsRequired(false).HasConstraintName("FK__Clinic__Manager");

			});

			modelBuilder.Entity<Degree>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Degree");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasIndex(e => e.Name).IsUnique();
			});

			modelBuilder.Entity<Dentist>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Dentist");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasOne(d => d.Account).WithOne(p => p.Dentists).IsRequired(false).HasConstraintName("FK__Dentist__Account").OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(d => d.Clinic).WithMany(p => p.Dentists).HasConstraintName("FK__Dentist__Clinic").OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(d => d.Degree).WithMany(p => p.Dentists).HasConstraintName("FK__Dentist__Degree").OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<DentistSpecialty>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_DentistSpecialty");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasOne(d => d.Dentist).WithMany(p => p.DentistSpecialties).HasConstraintName("FK__DentistSpecialty__Dentist").OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(d => d.Specialty).WithMany(p => p.DentistSpecialties).HasConstraintName("FK__DentistSpecialty__Specialty").OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<PatientRecord>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Patient");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.Property(e => e.MemberCard).IsFixedLength();

				entity.HasIndex(e => e.MemberCard).IsUnique();

				entity.HasOne(d => d.Account).WithMany(p => p.PatientRecords).IsRequired(false).HasConstraintName("FK__Patient__Account").OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<Review>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Review");
				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasOne(d => d.Dentist).WithMany(p => p.Reviews).HasConstraintName("FK__Review__Dentist").OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(d => d.Patient).WithMany(p => p.Reviews).HasConstraintName("FK__Review__Patient").OnDelete(DeleteBehavior.Restrict);
			});

			modelBuilder.Entity<Schedule>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Schedule");
				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasOne(d => d.Dentist).WithMany(p => p.Schedules).HasConstraintName("FK__Schedule__Dentis").OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(d => d.TimeSlot).WithMany(p => p.Schedules).HasConstraintName("FK__Schedule__TimeSlot").OnDelete(DeleteBehavior.Restrict);

				entity.HasCheckConstraint("CK_Valid_Schedule_Status", "ScheduleStatus = 'Booked' OR ScheduleStatus = 'Available' OR ScheduleStatus = N'Đã Đặt' OR ScheduleStatus = N'Còn Trống'");
			});

			modelBuilder.Entity<Service>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Service");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasIndex(e => e.Name).IsUnique();

				entity.HasOne(d => d.Clinic).WithMany(p => p.Services).HasConstraintName("FK__Service__Clinic").OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(d => d.Specialty).WithMany(p => p.Services).HasConstraintName("FK__Service__Special").OnDelete(DeleteBehavior.Cascade);
			});

			modelBuilder.Entity<Specialty>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Specialty");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.HasIndex(e => e.Name).IsUnique();
			});

			modelBuilder.Entity<TimeSlot>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_TimeSlot");
				entity.Property(e => e.ID).ValueGeneratedOnAdd();
			});

			modelBuilder.Entity<Transaction>(entity =>
			{
				entity.HasKey(e => e.ID).HasName("PK_Transaction");

				entity.Property(e => e.ID).ValueGeneratedOnAdd();

				entity.Property(e => e.BankAccountNumber).IsFixedLength();

				entity.HasOne(d => d.Appointment).WithMany(p => p.Transactions).HasConstraintName("FK__Transacti__Appointment").OnDelete(DeleteBehavior.NoAction);
			});

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}