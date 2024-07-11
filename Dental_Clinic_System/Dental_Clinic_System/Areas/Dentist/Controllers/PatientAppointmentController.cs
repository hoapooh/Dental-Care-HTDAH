using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
	[Area("dentist")]
	[Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
	public class PatientAppointmentController : Controller
	{
		private readonly DentalClinicDbContext _context;
		public PatientAppointmentController(DentalClinicDbContext context)
		{
			_context = context;
		}

		#region Lấy dữ liệu quản lý lịch đặt khám của bệnh nhân cho nha sĩ

		[HttpGet]
		public async Task<IActionResult> PatientAppointments(int? appointmentID, string? status)
		{
			var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
			if (dentistAccountID == null)
			{
				return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
			}

			var dentist = await _context.Dentists.Where(d => d.Account.ID == dentistAccountID).Include(d => d.Account).FirstAsync();
			var appointments = await _context.Appointments
									.Include(a => a.Schedule).ThenInclude(s => s.TimeSlot)
									.Include(a => a.PatientRecords)
									.Include(a => a.Specialty)
									.Where(a => a.Schedule.DentistID == dentist.ID)
									.ToListAsync();
			if (appointmentID != null)
			{
				appointments = appointments.Where(a => a.ID == appointmentID).ToList();
			}
			if (status != null)
			{
				appointments = appointments.Where(a => a.AppointmentStatus == status).ToList();
			}
			ViewBag.DentistID = dentist.ID;
			ViewBag.DentistAvatar = dentist?.Account.Image;
			ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
			TempData["ErrorMessage"] = TempData["ErrorMessage"] as string;
			TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
			return View(appointments);
		}




		//[HttpPost]
		//public async Task<IActionResult> ChangePatientAppointment(int appointmentID, string appointmentStatus)
		//{
		//    var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
		//    if (dentistAccountID == null)
		//    {
		//        return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
		//    }
		//    var appointment = await _context.Appointments.Include(a => a.Transactions).Where(a => a.ID == appointmentID).FirstOrDefaultAsync();
		//    appointment.AppointmentStatus = appointmentStatus;

		//    _context.Update(appointment);
		//    await _context.SaveChangesAsync();

		//    var schedule = await _context.Appointments.Include(s => s.Schedule).FirstOrDefaultAsync(a => a.ID == appointmentID);
		//    if (appointment.AppointmentStatus == "Đã Khám" || appointment.AppointmentStatus == "Đã Hủy")
		//    {
		//        schedule.Schedule.ScheduleStatus = "Còn Trống";
		//        await _context.SaveChangesAsync();
		//    }

		//    // HERE
		//    #region Refund MOMO API
		//    /*if (appointment.AppointmentStatus == "Đã Khám")
		//    {
		//        // Invoke the refund method from payment controller
		//        // Hoàn tiền thành công
		//        //return View("RefundSuccess", responseObject);
		//        var transactionCode = appointment.Transactions.FirstOrDefault()?.TransactionCode;
		//        var amount = appointment.Transactions.FirstOrDefault()?.TotalPrice;
		//        var bankName = appointment.Transactions.FirstOrDefault()?.BankName;
		//        var fullName = appointment.Transactions.FirstOrDefault()?.FullName;
		//        if (_momoPayment.RefundPayment((long)decimal.Parse(amount.ToString()), long.Parse(transactionCode.ToString()), "") != null)
		//        {
		//            TempData["RefundMessage"] = "Hoàn tiền thành công";

		//            var transaction = new Transaction
		//            {
		//                AppointmentID = appointment.ID,
		//                Date = DateTime.Now,
		//                BankName = bankName,
		//                TransactionCode = transactionCode,
		//                PaymentMethod = "MOMO",
		//                TotalPrice = amount,
		//                BankAccountNumber = "9704198526191432198",
		//                FullName = fullName,
		//                Message = "Hoàn tiền thành công",
		//                Status = "Thành Công"
		//            };

		//            _context.Transactions.Add(transaction);
		//            _context.SaveChanges();
		//        }

		//    }*/
		//    #endregion

		//    TempData["message"] = "success";
		//    return RedirectToAction("PatientAppointments");
		//}
		#endregion

		#region Hàm xử lý nút bấm thay đổi trạng thái đơn khám của bệnh nhân với dentist tương ứng
		//Hàm hủy đơn đặt
		public async Task<IActionResult> CancelAppointment(int appointmentID, string? description)  //string description
		{
			var appointment = _context.Appointments.FirstOrDefault(a => a.ID == appointmentID && (a.AppointmentStatus == "Đã Chấp Nhận" || a.AppointmentStatus == "Chờ Xác Nhận"));
			if (appointment == null)
			{
				TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
				return RedirectToAction("patientappointments");
			}
			appointment.AppointmentStatus = "Đã Hủy";
			appointment.Description = "Đã hủy từ Nha sĩ. Lý do hủy: " + description;
			_context.Update(appointment);
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
			return RedirectToAction("patientappointments");
		}

		//Hàm thay đổi trạng thái của đơn đặt
		public async Task<IActionResult> ChangeStatusAppointment(int appointmentID, int statusNumber)
		{
			var appointment = _context.Appointments.FirstOrDefault(a => a.ID == appointmentID && (a.AppointmentStatus == "Chờ Xác Nhận" || a.AppointmentStatus == "Đã Chấp Nhận"));
			if (appointment == null)
			{
				TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
				return RedirectToAction("patientappointments");
			}

			if (statusNumber == 1)
			{
				appointment.AppointmentStatus = "Đã Chấp Nhận";
			}
			else if (statusNumber == 2)
			{
				appointment.AppointmentStatus = "Đã Khám";
			}
			_context.Update(appointment);
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
			return RedirectToAction("patientappointments");
		}
		#endregion


	}
}
