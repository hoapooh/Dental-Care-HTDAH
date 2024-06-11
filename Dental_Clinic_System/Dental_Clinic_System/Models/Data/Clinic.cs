using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Models.Data
{
    [Table("Clinic")]
    public partial class Clinic
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Column("ManagerID")]
        public int ManagerID { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Column("Province")]
        public int? Province { get; set; } = null!;

		[Column("Ward")]
		public int? Ward { get; set; } = null!;

		[Column("District")]
		public int? District { get; set; } = null!;

		[Column("Address", TypeName = "nvarchar(200)")]
        public string Address { get; set; } = null!;

        [Column("Basis",TypeName = "nvarchar(200)")]
        public string? Basis { get; set; } = null;

        [Column("PhoneNumber", TypeName = "varchar(11)")]
        public string? PhoneNumber { get; set; }

        [Column("Email", TypeName = "varchar(50)")]
        public string? Email { get; set; }

        [Column("Description",TypeName = "ntext")]
        public string? Description { get; set; }

        [Column("Image", TypeName = "varchar(256)")]
        public string Image { get; set; } = null!;

        [Column("ClinicStatus", TypeName = "nvarchar(30)")]
        public string ClinicStatus { get; set; } = null!;

        [Column("MapLinker", TypeName = "ntext")]
        public string? MapLinker { get; set; }

        #region Foreign Key

        [ForeignKey("ManagerID")]
		[InverseProperty("Clinics")]
		public virtual Account Manager { get; set; } = null!;

		#endregion

		#region Entity Mapping

		[InverseProperty("Clinic")]
        public virtual ICollection<Dentist> Dentists { get; set; } = new List<Dentist>();

        [InverseProperty("Clinic")]
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();

        #endregion

        
	}
}