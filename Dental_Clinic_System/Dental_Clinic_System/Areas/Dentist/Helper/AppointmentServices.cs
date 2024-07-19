using Dental_Clinic_System.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Dentist.Helper
{
	public static class AppointmentServices
	{
		/// <summary>
		/// Định dạng lại thời gian cho phù hợp với Database với format "MM/đ/yyyy"
		/// </summary>
		/// <param name="date"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="desiredDate"></param>
		/// <param name="startTime"></param>
		/// <param name="endTime"></param>
		public static void FormatDateTime(List<DateOnly> date, string start, string end, out List<DateOnly> desiredDate, out TimeOnly startTime, out TimeOnly endTime)
		{
			desiredDate = date;
			startTime = TimeOnly.ParseExact(start, "HH:mm");
			endTime = TimeOnly.ParseExact(end, "HH:mm");
		}

		/// <summary>
		/// Lấy danh sách ngày đã được đặt của nha sĩ và kiểm tra xem (những) ngày đã chọn có trùng với ngày đã đặt không
		/// </summary>
		/// <param name="chosenDate">List các ngày đã chọn</param>
		/// <param name="startTime">Thời gian đã chọn</param>
		/// <param name="dentistID">ID của Dentist</param>
		/// <param name="_context">DentalClinicDbContext</param>
		/// <returns>Trả về giá trị boolean, nếu như chọn ngày trùng với ngày đặt thì trả về false, nguọc lại thì trả về true</returns>
		public static bool IsHaveSelectedDate(List<DateOnly> chosenDate, TimeOnly startTime, int dentistID, DentalClinicDbContext _context)
		{
			//var schedules = _context.Schedules
			//	.Include(s => s.TimeSlot)
			//	.Where(s => s.DentistID == dentistID)
			//.ToList();

			var appointments = _context.Appointments
				.Include(a => a.Schedule)
					.ThenInclude(s => s.TimeSlot)
				.Where(a => a.Schedule.DentistID == dentistID)
			.ToList();

			var futureAppointments = _context.PeriodicAppointments
				.Include(f => f.Dentist)
				.Where(f => f.Dentist_ID == dentistID)
				.ToList();

			var dates = new List<DateTime>();

			//foreach (var schedule in schedules)
			//{
			//	if (schedule.TimeSlotID != 1 && schedule.TimeSlotID != 2)
			//	{
			//		dates.Add(schedule.Date.ToDateTime(schedule.TimeSlot.StartTime));
			//	}
			//}
			foreach (var appointment in appointments)
			{
				dates.Add(appointment.Schedule.Date.ToDateTime(appointment.Schedule.TimeSlot.StartTime));
			}
			foreach (var futureAppointment in futureAppointments)
			{
				dates.Add(futureAppointment.DesiredDate.ToDateTime(futureAppointment.StartTime));
			}

			foreach (var date in chosenDate)
			{
				foreach (var dateInList in dates)
				{

					if (dateInList == date.ToDateTime(startTime))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
