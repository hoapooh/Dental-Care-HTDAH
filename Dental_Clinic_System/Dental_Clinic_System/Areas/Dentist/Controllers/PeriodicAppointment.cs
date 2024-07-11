using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
	[Area("dentist")]
	[Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
	public class PeriodicAppointment : Controller
	{
		private readonly DentalClinicDbContext _context;
		public PeriodicAppointment(DentalClinicDbContext context)
		{
			_context = context;
		}


		#region Hàm lấy thông tin của tất cả lịch điều trị cho nha sĩ đó - FUTURE APPOINTMENT
		[HttpGet]
		//[Route("periodicappointment")]
		public async Task<IActionResult> ShowPeriodicAppointment(int? periodicappointmentID, string? keyword)
		{
			var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
			if (dentistAccountID == null)
			{
				return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
			}

			var dentist = await _context.Dentists.Where(d => d.Account.ID == dentistAccountID).Include(d => d.Account).FirstAsync();

			var periodicAppointment = await _context.FutureAppointments
									.Include(p => p.PatientRecord)
									.Include(p => p.Dentist)
									.Where(p => p.Dentist.Account.ID == dentistAccountID)
									.ToListAsync();

			if (!string.IsNullOrEmpty(keyword))
			{
				keyword = keyword.Trim().ToLower();
				keyword = Util.ConvertVnString(keyword);
				periodicAppointment = periodicAppointment.Where(p =>
					Util.ConvertVnString(p.PatientRecord.FullName).Contains(keyword) ||
					Util.ConvertVnString(p.StartTime.ToString("HH:mm")).Contains(keyword) ||
					Util.ConvertVnString(p.EndTime.ToString("HH:mm")).Contains(keyword) ||
					Util.ConvertVnString(p.DesiredDate.ToString("dd/MM/yyyy")).Contains(keyword))
					.ToList();
				TempData["ErrorMessage"] = "Không tìm thấy kết quả tương ứng!";
			}

			ViewBag.DentistAvatar = dentist?.Account.Image ?? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FPatient%2Fuser.png?alt=media&token=9010a4a6-0220-4d29-bb85-1fe425100744";
			ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
			return View("PeriodicAppointment", periodicAppointment);
		}
		#endregion

		#region Hàm xử lý nút bấm thay đổi trạng thái của lịch khám định kì - FUTURE APPOINTMENT
		public async Task<IActionResult> CancelFutureAppointment(int futureappointmentID, string? description)  //string description
		{
			var futureAppointment = _context.FutureAppointments.FirstOrDefault(a => a.ID == futureappointmentID && a.FutureAppointmentStatus == "Chưa Khám");
			if (futureAppointment == null)
			{
				TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
				return RedirectToAction("showperiodicappointment");
			}
			futureAppointment.FutureAppointmentStatus = "Đã Hủy";
			//futureAppointment.Description = "Đã hủy từ Nha sĩ. Lý do hủy: " + description;
			_context.Update(futureAppointment);
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
			return RedirectToAction("showperiodicappointment");
		}

		//Hàm thay đổi trạng thái của đơn đặt
		public async Task<IActionResult> ChangeStatusFutureAppointment(int futureappointmentID)
		{
			var futureappointment = _context.FutureAppointments.FirstOrDefault(a => a.ID == futureappointmentID && a.FutureAppointmentStatus == "Chưa Khám");
			if (futureappointment == null)
			{
				TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
				return RedirectToAction("showperiodicappointment");
			}

			futureappointment.FutureAppointmentStatus = "Đã Khám";

			_context.Update(futureappointment);
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
			return RedirectToAction("showperiodicappointment");
		}
		#endregion
	}
}
