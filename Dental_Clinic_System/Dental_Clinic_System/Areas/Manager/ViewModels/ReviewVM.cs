using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
	public class ReviewVM
	{
		public int ID { get; set; }

		public int DentistID { get; set; }
		public string DentistName { get; set; }

		public int PatientID { get; set; }
		public string PatientName { get; set; }

		public double? Rating { get; set; } = null;

		public string Comment { get; set; } = null!;

		public DateOnly Date { get; set; }
	}
}
