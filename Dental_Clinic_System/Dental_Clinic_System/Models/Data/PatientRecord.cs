﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models.Data
{
    [Table("PatientRecord")]
    public partial class PatientRecord
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("AccountID")]
        public int AccountID { get; set; }

		[Column("MemberCard",TypeName ="nvarchar(15)")]
        public string? MemberCard { get; set; }

        [Column("FullName", TypeName ="nvarchar(75)")]
        public string FullName { get; set; } = null!;

		[Column("DateOfBirth", TypeName = "date")]
		public DateOnly DateOfBirth { get; set; }

		[Column("PhoneNumber", TypeName = "varchar(11)")]
		public string? PhoneNumber { get; set; } = null;

		[Column("Gender", TypeName = "nvarchar(6)")]
		public string Gender { get; set; } = null!;

		[Column("Job", TypeName = "nvarchar(50)")]
		public string Job { get; set; } = null!;

		[Column("IdentityNumber", TypeName = "varchar(12)")]
		public string? IdentityNumber { get; set; } = null;

		[Column("EmailReceiver", TypeName = "varchar(50)")]
		public string? EmailReceiver { get; set; } = null;

		[Column("Province", TypeName = "nvarchar(50)")]
		public int? Province { get; set; } = null!;

		[Column("District")]
		public int? District { get; set; } = null!;

		[Column("Ward")]
		public int? Ward { get; set; } = null!;

		[Column("Address", TypeName = "nvarchar(50)")]
		public string Address { get; set; } = null!;

		//Family Member------------------------------------------

		[Column("FMName", TypeName = "nvarchar(75)")]
		public string? FMName { get; set; } = null;

		[Column("FMRelationship", TypeName = "nvarchar(30)")]
		public string? FMRelationship { get; set; } = null;

		[Column("FMPhoneNumber", TypeName = "varchar(11)")]
		public string? FMPhoneNumber { get; set; } = null;

		#region Foreign Key

		[ForeignKey("AccountID")]
        [InverseProperty("PatientRecords")]
        public virtual Account? Account { get; set; }
		#endregion

		#region Entity Mapping

		[InverseProperty("PatientRecords")]
        public virtual ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();
		#endregion
	}
}