using Dental_Clinic_System.Models.Data;

namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
	public class FutureAppointmentVM
	{
		//để hỗ trợ xác định những lịch "Đã Đặt" khi tạo lịch
		public int DentistID { get; set; }
		public DateOnly Date { get; set; }
		public List<int> Slots { get; set; }
	}
}
