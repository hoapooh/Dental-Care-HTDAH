using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dental_Clinic_System.Models.Data
{
	public class FutureAppointment
	{
		[Key]
		public int ID { get; set; }

		[Column("PatientRecord_ID", TypeName = "int")]
		public int PatientRecord_ID { get; set; }

		[Column("Dentist_ID", TypeName = "int")]
		public int Dentist_ID { get; set; }

		[Column("StartTime", TypeName = "time(7)")]
		public TimeOnly StartTime { get; set; }

		[Column("EndTime", TypeName = "time(7)")]
		public TimeOnly EndTime { get; set; }

		[Column("DesiredDate", TypeName = "date")]
		public DateOnly DesiredDate { get; set; }

		[Column("FutureAppointmentStatus", TypeName = "nvarchar(30)")]
		public string? FutureAppointmentStatus { get; set; }

		#region Foreign Key
		[ForeignKey("PatientRecord_ID")]
		[InverseProperty("FutureAppointments")]
		public virtual PatientRecord? PatientRecord { get; set; }

		[ForeignKey("Dentist_ID")]
		[InverseProperty("FutureAppointments")]
		public virtual Dentist? Dentist { get; set; }
		#endregion
	}
}
