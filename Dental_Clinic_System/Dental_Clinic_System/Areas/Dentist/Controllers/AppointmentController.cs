using Dental_Clinic_System.Areas.Dentist.Helper;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
    [Area("dentist")]
    [Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
    public class AppointmentController : Controller
    {
        private readonly DentalClinicDbContext _context;
        public AppointmentController(DentalClinicDbContext context)
        {
            _context = context;
        }
        //===================APPOINTMENT===================
        #region Lấy dữ liệu quản lý lịch đặt khám của bệnh nhân cho nha sĩ

        [HttpGet]
        public async Task<IActionResult> PatientAppointment(int? appointmentID, string? status, string? keyword)
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
                                    .Include(a => a.Transactions)
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
            if (!string.IsNullOrEmpty(keyword))
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
                return RedirectToAction("patientappointment");
            }
            appointment.AppointmentStatus = "Đã Hủy";
            appointment.Description = "Đã hủy từ Nha sĩ. Lý do hủy: " + description;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
            return RedirectToAction("patientappointment");
        }

        //Hàm thay đổi trạng thái của đơn đặt
        public async Task<IActionResult> ChangeStatusAppointment(int appointmentID, int statusNumber)
        {
            var appointment = _context.Appointments.Include(a => a.Schedule)
                                                    .ThenInclude(s => s.TimeSlot)   
                                                    .FirstOrDefault(a => a.ID == appointmentID && (a.AppointmentStatus == "Chờ Xác Nhận" || a.AppointmentStatus == "Đã Chấp Nhận"));
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
                return RedirectToAction("patientappointment");
            }

            DateTime now = Util.GetUtcPlus7Time();
            var dateTimeStart = appointment.Schedule.Date.ToDateTime(appointment.Schedule.TimeSlot.StartTime).AddMinutes(5);

			if (statusNumber == 1)
            {
                appointment.AppointmentStatus = "Đã Chấp Nhận";
            }
            else if (statusNumber == 2)
            {
                if (dateTimeStart > now) // để được bấm Đã Khám thì thời gian hiện tại phải lớn hơn thời gian bắt đầu khams 5 phút
				{
                    TempData["ErrorMessage"] = "Chưa tới thời gian cho phép thay đổi trạng thái này!";
                    return RedirectToAction("patientappointment");
                }
                appointment.AppointmentStatus = "Đã Khám";
            }
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
            return RedirectToAction("patientappointment");
        }
		#endregion

		#region Hàm handle những data truyền vào có thể xuất ra future appointment hay không
		[HttpPost]
		public async Task<IActionResult> ValidAppointmentForFutureAppointment(int appointmentID, string? ngayhentaikham, string? giobatdau, string? gioketthuc, string ketquakham, string?dando)
			{
			var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
			if (dentistAccountID == null)
			{
				return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
			}

			// Lấy thông tin Nha sĩ
			var dentist = await _context.Dentists
										.Include(d => d.Account)
										.Include(d => d.Clinic)
										.FirstAsync(d => d.Account.ID == dentistAccountID);
            var dentistID = dentist.ID;

			var appointment = _context.Appointments
				.Include(a => a.Schedule)
				.ThenInclude(s => s.Dentist)
				.Where(a => a.ID == appointmentID)
				.FirstOrDefault();

            if(appointment == null)
            {
                return Redirect("/Home/Error");
            }

			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new DateOnlyConverter());

			List<DateOnly> selectedDates;
			try
			{
				selectedDates = JsonConvert.DeserializeObject<List<DateOnly>>(ngayhentaikham, settings);
			}
			catch (JsonSerializationException ex)
			{
				TempData["ErrorMessage"] = $"Lỗi khi phân tích ngày hẹn tái khám: {ex.Message}";
				return RedirectToAction("patientappointment");
			}

			TimeOnly startTimeInCalendar;
			try
			{
				startTimeInCalendar = TimeOnly.ParseExact(giobatdau, "HH:mm");
			}
			catch (FormatException ex)
			{
				TempData["ErrorMessage"] = $"Lỗi khi phân tích giờ bắt đầu: {ex.Message}";
				return RedirectToAction("patientappointment");
			}

			if (!AppointmentServices.IsHaveSelectedDate(selectedDates, startTimeInCalendar, dentistID, _context))
			{
				TempData["ErrorMessage"] = "Bạn đã có ca làm việc tương ứng với thời gian này, vui lòng chọn lại!";
				return RedirectToAction("patientappointment");
			}

			//Lấy ngày và giờ riêng ra của hentaikham
			AppointmentServices.FormatDateTime(selectedDates, giobatdau, gioketthuc, out List<DateOnly> desiredDate, out TimeOnly startTime, out TimeOnly endTime);

			//Thêm appointment với giờ tạo vào PeriodicAppointments 
			if (appointment.IsExport == false) // Case: lần đầu tiên tạo future appointment
			{
				appointment.Note = $"{dando}";
				appointment.Description = $"{ketquakham}";
				appointment.IsExport = true;
				if (!string.IsNullOrEmpty(ngayhentaikham))
				{

					// Lặp qua mỗi ngày trong danh sách ngày hẹn tái khám, tạo future appointment cho mỗi ngày
					foreach (var date in desiredDate)
					{
						var futureAppointment = new PeriodicAppointment
						{
							PatientRecord_ID = appointment.PatientRecordID,
							Dentist_ID = dentistID,
							StartTime = startTime,
							EndTime = endTime,
							DesiredDate = date,
							PeriodicAppointmentStatus = "Chưa Khám",
							AppointmentID = appointmentID
						};
						_context.PeriodicAppointments.Add(futureAppointment);
					}
				}
				_context.SaveChanges();
			}


            else // Case: đã tạo future appointment trước đó
            {
				appointment.Note = $"Dặn dò: {dando}";
				appointment.Description = $"Đã Khám. Kết quả Khám: {ketquakham}";
				if (!string.IsNullOrEmpty(ngayhentaikham))
                {

                    //Lấy FutureAppointment để cập nhật lại thời gian tái khám
                    var futureAppointments = _context.PeriodicAppointments.Where(fa => fa.AppointmentID == appointmentID && fa.PeriodicAppointmentStatus == "Chưa Khám").ToList();
                    if (futureAppointments == null)
                    {
                        return NotFound("Không tìm thấy đơn!");
                    }
                    // Xóa các futureAppointment cũ
					foreach (var futureAppointment in futureAppointments)
					{
						_context.PeriodicAppointments.Remove(futureAppointment);
					}
					// Thêm các futureAppointment mới
					foreach (var date in desiredDate)
					{
						// Create a new FutureAppointment instance for each date
						var futureAppointment = new PeriodicAppointment
						{
							PatientRecord_ID = appointment.PatientRecordID,
							Dentist_ID = dentistID,
							StartTime = startTime, 
							EndTime = endTime,
							DesiredDate = date,
							PeriodicAppointmentStatus = "Chưa Khám",
							AppointmentID = appointmentID
						};

						_context.PeriodicAppointments.Add(futureAppointment);
					}
				}
				_context.SaveChanges();
			}

            //=========================================================================


            return RedirectToAction("generatepdf","appointmentpdf", new {dentistID, appointmentID, selectedDates, giobatdau, gioketthuc, ketquakham, dando});
			}


        #endregion

        //===================PERIODIC APPOINTMENT===================

        #region Hàm lấy thông tin của tất cả lịch điều trị cho nha sĩ đó - FUTURE APPOINTMENT
        [HttpGet]
        public async Task<IActionResult> PeriodicAppointment(int? periodicappointmentID, string? keyword)
        {
            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
            }

            var dentist = await _context.Dentists.Where(d => d.Account.ID == dentistAccountID).Include(d => d.Account).FirstAsync();

            var periodicAppointment = await _context.PeriodicAppointments
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
            return View(periodicAppointment);
        }
        #endregion

        #region Hàm xử lý nút bấm thay đổi trạng thái của lịch khám định kì - FUTURE APPOINTMENT
        public async Task<IActionResult> CancelFutureAppointment(int futureappointmentID, string? description)
        {
            var futureAppointment = _context.PeriodicAppointments.FirstOrDefault(a => a.ID == futureappointmentID && a.PeriodicAppointmentStatus == "Chưa Khám");
            if (futureAppointment == null)
            {
                TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
                return RedirectToAction("periodicappointment");
            }
            futureAppointment.PeriodicAppointmentStatus = "Đã Hủy";
            //futureAppointment.Description = "Đã hủy từ Nha sĩ. Lý do hủy: " + description;
            _context.Update(futureAppointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
            return RedirectToAction("periodicappointment");
        }

        //Hàm thay đổi trạng thái của đơn đặt
        public async Task<IActionResult> ChangeStatusFutureAppointment(int futureappointmentID)
        {
            var futureAppointment = _context.PeriodicAppointments.FirstOrDefault(a => a.ID == futureappointmentID && a.PeriodicAppointmentStatus == "Chưa Khám");
            if (futureAppointment == null)
            {
                TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
                return RedirectToAction("periodicappointment");
            }
			DateTime now = Util.GetUtcPlus7Time();
			var dateTimeStart = futureAppointment.DesiredDate.ToDateTime(futureAppointment.StartTime).AddMinutes(5);
			if (dateTimeStart > now) // để được bấm Đã Khám thì thời gian hiện tại phải lớn hơn thời gian bắt đầu khams 5 phút
			{
				TempData["ErrorMessage"] = "Chưa tới thời gian cho phép thay đổi trạng thái này!";
				return RedirectToAction("periodicappointment");
			}

			futureAppointment.PeriodicAppointmentStatus = "Đã Khám";

            _context.Update(futureAppointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
            return RedirectToAction("periodicappointment");
        }
        #endregion


    }
}
