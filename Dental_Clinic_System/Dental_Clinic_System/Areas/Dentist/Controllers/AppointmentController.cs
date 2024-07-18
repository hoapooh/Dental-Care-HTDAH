using Dental_Clinic_System.Areas.Dentist.Helper;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.ProjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
	[Area("dentist")]
	[Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
	public class AppointmentController : Controller
	{
		private readonly DentalClinicDbContext _context;
		private readonly IEmailSenderCustom _emailSender;
		public AppointmentController(DentalClinicDbContext context, IEmailSenderCustom emailSender)
		{
			_context = context;
			_emailSender = emailSender;
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
									.Include(a => a.Schedule)
										.ThenInclude(s => s.TimeSlot)
									.Include(a => a.PatientRecords)
										.ThenInclude(pr => pr.PeriodicAppointments)
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
				keyword.Trim().ToLower();
				keyword = Util.ConvertVnString(keyword);
				appointments = appointments.Where(a => (a.Transactions.Where(t => t.AppointmentID == a.ID).First().MedicalReportID != null && Util.ConvertVnString(a.Transactions.Where(t => t.AppointmentID == a.ID).First().MedicalReportID).Contains(keyword)) ||
														(a.PatientRecords.FullName != null && Util.ConvertVnString(a.PatientRecords.FullName).Contains(keyword)) ||
														(a.Specialty.Name != null && Util.ConvertVnString(a.Specialty.Name).Contains(keyword)) ||
														(a.Schedule.Date.ToString("dd/MM/yyyy") != null && Util.ConvertVnString(a.Schedule.Date.ToString("dd/MM/yyyy")).Contains(keyword)) ||
														((a.Schedule.TimeSlot.StartTime.ToString("HH:mm") != null && a.Schedule.TimeSlot.EndTime.ToString("HH:mm") != null) && Util.ConvertVnString(a.Schedule.TimeSlot.StartTime.ToString("HH:mm") + " - " + a.Schedule.TimeSlot.EndTime.ToString("HH:mm")).Contains(keyword)) ||
														(a.CreatedDate != null && Util.ConvertVnString(DateOnly.FromDateTime(a.CreatedDate ?? new DateTime()).ToString("dd/MM/yyyy")).Contains(keyword))

														  ).ToList();
			}
			ViewBag.DentistID = dentist.ID;
			ViewBag.DentistAvatar = dentist?.Account.Image;
			ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
			ViewBag.Status = status;
			TempData["ErrorMessage"] = TempData["ErrorMessage"] as string;
			TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
			return View(appointments);
		}
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
					TempData["ErrorMessage"] = "Chưa tới thời gian hẹn khám, không cho phép thay đổi trạng thái này!";
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

		                      #region Hàm handle những data truyền vào có thể xuất ra future appointment (PDF) hay không
		[HttpPost]
		public async Task<IActionResult> ValidAppointmentForFutureAppointment(int appointmentID, string? ngayhentaikham, string? giobatdau, string? gioketthuc, string ketquakham, string? dando)
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
				.Include(a => a.PatientRecords)
					.ThenInclude(p => p.Account)
				.Where(a => a.ID == appointmentID)
				.FirstOrDefault();

			var appointmentAvailable = _context.Appointments
				.Include(a => a.Schedule)
					.ThenInclude(s => s.TimeSlot)
				.Where(a => a.AppointmentStatus != "Đã Hủy")
				.ToList();

			var periodicAppointmentsAvailable = _context.PeriodicAppointments
				.Where(p => p.AppointmentID != appointmentID && p.PeriodicAppointmentStatus == "Đã Chấp Nhận")
				.ToList();

			List<DateOnly>? selectedDates = null;

			if (appointment == null)
			{
				return Redirect("/Home/Error");
			}

			if (ngayhentaikham == null)
			{
				appointment.Note = $"{dando}";
				appointment.Description = $"{ketquakham}";
				appointment.IsExport = true;

			}

			else { 
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new DateOnlyConverter());

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
				TimeOnly endTimeInCalendar;
				try
				{
					startTimeInCalendar = TimeOnly.ParseExact(giobatdau, "HH:mm");
					endTimeInCalendar = TimeOnly.ParseExact(gioketthuc, "HH:mm");
				}
				catch (FormatException ex)
				{
					TempData["ErrorMessage"] = $"Lỗi khi phân tích giờ bắt đầu: {ex.Message}";
					return RedirectToAction("patientappointment");
				}

				if (startTimeInCalendar >= endTimeInCalendar)
				{
					TempData["ErrorMessage"] = "Giờ kết thúc phải lớn hơn giờ bắt đầu!";
					return RedirectToAction("patientappointment");
				}

				//Valid ngày hẹn tái khám xem có trùng với ngày hẹn khác + periodic không
				foreach (var appoint in appointmentAvailable)
				{
					DateTime start = appoint.Schedule.Date.ToDateTime(appoint.Schedule.TimeSlot.StartTime);
					DateTime end = appoint.Schedule.Date.ToDateTime(appoint.Schedule.TimeSlot.EndTime);
					foreach (var date in selectedDates)
					{
						if((date.ToDateTime(startTimeInCalendar) < end && date.ToDateTime(endTimeInCalendar) > end) ||
						   (date.ToDateTime(startTimeInCalendar) < start && date.ToDateTime(endTimeInCalendar) > start) ||
						   (date.ToDateTime(startTimeInCalendar) <= start && date.ToDateTime(endTimeInCalendar) <= end)
							)
						{
							TempData["ErrorMessage"] = $"Ngày hẹn tái khám trùng với ngày {appoint.Schedule.Date.ToString("dd/MM/yyyy")} lúc {appoint.Schedule.TimeSlot.StartTime.ToString("HH:mm")} - {appoint.Schedule.TimeSlot.EndTime.ToString("HH:mm")}!";
							return RedirectToAction("patientappointment");
						}
					}
				}

				foreach (var appoint in periodicAppointmentsAvailable)
				{
					DateTime start = appoint.DesiredDate.ToDateTime(appoint.StartTime);
					DateTime end = appoint.DesiredDate.ToDateTime(appoint.EndTime);
					foreach (var date in selectedDates)
					{
						if ((date.ToDateTime(startTimeInCalendar) < end && date.ToDateTime(endTimeInCalendar) > end) ||
						   (date.ToDateTime(startTimeInCalendar) < start && date.ToDateTime(endTimeInCalendar) > start) ||
						   (date.ToDateTime(startTimeInCalendar) <= start && date.ToDateTime(endTimeInCalendar) <= end)
							)
						{
							TempData["ErrorMessage"] = $"Ngày hẹn tái khám trùng với ngày {appoint.DesiredDate.ToString("dd/MM/yyyy")} lúc {appoint.StartTime.ToString("HH:mm")} - {appoint.EndTime.ToString("HH:mm")}!";
							return RedirectToAction("patientappointment");
						}
					}
				}

				if (!AppointmentServices.IsHaveSelectedDate(selectedDates, startTimeInCalendar, dentistID, _context))
				{
					TempData["ErrorMessage"] = "Bạn đã có ca làm việc tương ứng trong thời gian này, vui lòng chọn lại!";
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

					// Lặp qua mỗi ngày trong danh sách ngày hẹn tái khám, tạo periodic appointment cho mỗi ngày
					foreach (var date in desiredDate)
					{
						var newPeriodicAppointment = new PeriodicAppointment
						{
							PatientRecord_ID = appointment.PatientRecordID,
							Dentist_ID = dentistID,
							StartTime = startTime,
							EndTime = endTime,
							DesiredDate = date,
							PeriodicAppointmentStatus = "Đã Chấp Nhận",
							AppointmentID = appointmentID
						};
						_context.PeriodicAppointments.Add(newPeriodicAppointment);
					}
					// lưu lại
					_context.SaveChanges();
				}

				//=========================================================================
				//gửi mail khi bấm Xuất PDF
				await SendMailToPatient(appointment.PatientRecords.EmailReceiver ?? appointment.PatientRecords.FMEmail ?? appointment.PatientRecords.Account.Email,
					  appointment.PatientRecords.FullName,
					  selectedDates, startTime,
					  endTime, dentist.Clinic.Name,
					  dentist.Account.LastName + " " + dentist.Account.FirstName);
			}


			return RedirectToAction("generatepdf", "appointmentpdf", new { dentistID, appointmentID, selectedDates, giobatdau, gioketthuc, ketquakham, dando });
		}


		#endregion

		//=================== PERIODIC APPOINTMENT ===================

		#region Hàm lấy thông tin của tất cả lịch điều trị cho nha sĩ đó - FUTURE APPOINTMENT
		[HttpGet]
		public async Task<IActionResult> PeriodicAppointment(int? periodicappointmentID, string? keyword, int? appointmentID)
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
			if (appointmentID != null)
			{
				periodicAppointment.Where(p => p.AppointmentID == appointmentID).ToList();
			}

			if (!string.IsNullOrEmpty(keyword))
			{
				keyword = keyword.Trim().ToLower();
				keyword = Util.ConvertVnString(keyword);
				periodicAppointment = periodicAppointment.Where(p =>
					Util.ConvertVnString(p.PatientRecord.FullName).Contains(keyword) ||
					Util.ConvertVnString(p.DesiredDate.ToString("dd/MM/yyyy")).Contains(keyword) ||
					Util.ConvertVnString(p.StartTime.ToString("HH:mm") + " - " + p.EndTime.ToString("HH:mm")).Contains(keyword)
					).ToList();
			}

			ViewBag.DentistAvatar = dentist?.Account.Image ?? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FPatient%2Fuser.png?alt=media&token=9010a4a6-0220-4d29-bb85-1fe425100744";
			ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
			return View(periodicAppointment);
		}
		#endregion

		#region Hàm xử lý nút bấm thay đổi trạng thái của lịch khám định kì - FUTURE APPOINTMENT
		public async Task<IActionResult> CancelPeriodicAppointment(int periodicappointmentID, string? description)
		{
			var periodicAppointment = _context.PeriodicAppointments.FirstOrDefault(p => p.ID == periodicappointmentID && p.PeriodicAppointmentStatus == "Đã Chấp Nhận");
			if (periodicAppointment == null)
			{
				TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
				return RedirectToAction("periodicappointment");
			}
			periodicAppointment.PeriodicAppointmentStatus = "Đã Hủy";
			//futureAppointment.Description = "Đã hủy từ Nha sĩ. Lý do hủy: " + description;
			_context.Update(periodicAppointment);
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
			return RedirectToAction("periodicappointment");
		}

		//Hàm thay đổi trạng thái của đơn đặt
		public async Task<IActionResult> ChangeStatusPeriodicAppointment(int periodicappointmentID)
		{
			var periodicAppointment = _context.PeriodicAppointments.FirstOrDefault(a => a.ID == periodicappointmentID && a.PeriodicAppointmentStatus == "Đã Chấp Nhận");
			if (periodicAppointment == null)
			{
				TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
				return RedirectToAction("periodicappointment");
			}
			DateTime now = Util.GetUtcPlus7Time();
			var dateTimeStart = periodicAppointment.DesiredDate.ToDateTime(periodicAppointment.StartTime).AddMinutes(5);
			if (dateTimeStart > now) // để được bấm Đã Khám thì thời gian hiện tại phải lớn hơn thời gian bắt đầu khams 5 phút
			{
				TempData["ErrorMessage"] = "Chưa tới thời gian hẹn khám, không cho phép thay đổi trạng thái này!";
				return RedirectToAction("periodicappointment");
			}

			periodicAppointment.PeriodicAppointmentStatus = "Đã Khám";

			_context.Update(periodicAppointment);
			await _context.SaveChangesAsync();
			TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
			return RedirectToAction("periodicappointment");
		}
		#endregion

		//=================== SUPPORT METHOD ===================

		#region Hàm gửi mail cho bệnh nhân khi nhấn nút xuất PDF
		private Task SendMailToPatient(string email, string fullName, List<DateOnly> dateList, TimeOnly startTime, TimeOnly endTime, string clinicName, string dentistName)
		{
			string subject = "Thông báo thời gian điều trị/ tái khám định kỳ";

			StringBuilder dateListBuilder = new();
			foreach (var date in dateList)
			{
				dateListBuilder.Append($"<li>{date.ToString("dd/MM/yyyy")}</li>");
			}

			string message = $"<p>Xin chào <strong>{fullName}</strong></p>" +
							 $"<p>Đây là thông báo về những đơn hẹn điều trị/ tái khám định kỳ của quý khách.</p><br>" +
							 $"<p>Tại: <strong>{clinicName}</strong> - Nha sĩ: <strong>{dentistName}</strong></p>" +
							 $"<p>Thời gian bắt đầu: <strong>{startTime.ToString("HH:mm")} - {endTime.ToString("HH:mm")}</strong></p>" +
							 $"<ul>Ngày khám: <strong>{dateListBuilder.ToString()}</strong></ul>" +
							 $"<p>Vui lòng đến đúng thời gian trên để được hỗ trợ tốt nhất.</p>" +
							 $"<p>Trân trọng,</p>" +
							 $"<p>DentalCare</p><br>" +
							 @$"<img src='https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Dental%20Care%20Logo%2FDentalCare.png?alt=media&token=8854a154-1dde-4aa3-b573-f3c0aca83776' alt='logo DentalCare'>";

			_emailSender.SendEmailAsync(email, subject, message);
			return Task.CompletedTask;
		}
		#endregion
	}
}
