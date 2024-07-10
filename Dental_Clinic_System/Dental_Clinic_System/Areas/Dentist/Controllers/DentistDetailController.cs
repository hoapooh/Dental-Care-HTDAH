using Azure;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.MOMO;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
	[Area("dentist")]
	[Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]

	public class DentistDetailController : Controller
	{
		private readonly DentalClinicDbContext _context;
		//private readonly IMOMOPayment _momoPayment;
		public DentistDetailController(DentalClinicDbContext context, IMOMOPayment momoPayment)
		{
			_context = context;
			//_momoPayment = momoPayment;
		}

		public IActionResult Index()
		{
			TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
			return RedirectToAction("DentistSchedule");
		}



		#region Lấy lịch làm việc của Dentist, đưa vào Calendar
		[HttpGet]
		//[Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
		public async Task<IActionResult> DentistSchedule()
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
										.FirstOrDefaultAsync(d => d.Account.ID == dentistAccountID);

			#region Mã tiền đề
			//#region Gộp 3 bảng (left join) để lấy appointmentID và tên bệnh nhân sau đó xuất ra
			////// Lấy Schedule của dentist cụ thể
			////var schedules = await _context.Schedules
			////                        .Include(s => s.Dentist)
			////                        .Include(s => s.TimeSlot)
			////                        .Where(s => s.Dentist.Account.ID == dentistAccountID)
			////                        .ToListAsync();

			////// Map 2 bên Schedule với appointment, sau đó map những cột appointment ko null với patient record
			////var scheduleAppointments = from schedule in schedules
			////                           join appointment in _context.Appointments
			////                           on schedule.ID equals appointment.ScheduleID into appointmentGroup
			////                           from appointment in appointmentGroup.DefaultIfEmpty()
			////                           select new
			////                           {
			////                               appointmentStatus = appointment?.AppointmentStatus ?? "",
			////                               appointmentID = appointment?.ID ?? 0,
			////                               scheduleDate = schedule.Date,
			////                               patientID = appointment?.PatientRecordID ?? 0,
			////                               startTime = schedule?.TimeSlot?.StartTime,
			////                               endTime = schedule?.TimeSlot?.EndTime,
			////                               patientRecordID = appointment?.PatientRecordID ?? 0
			////                           };


			////var result = from sa in scheduleAppointments
			////             join patientRecord in _context.PatientRecords
			////             on sa.patientRecordID equals patientRecord.ID into patientGroup
			////             from patientRecord in patientGroup.DefaultIfEmpty()
			////             select new
			////             {
			////                 sa.appointmentStatus,
			////                 sa.appointmentID,
			////                 sa.scheduleDate,
			////                 sa.startTime,
			////                 sa.endTime,
			////                 patientName = patientRecord?.FullName ?? "No Patient"
			////             };


			////// Map to EventVM
			////var events = result.Select(s => new EventVM
			////{
			////    Title = s.appointmentID != 0 ? $"#{s.appointmentID} - {s.patientName}" : "Trống",
			////    Start = s.scheduleDate != null && s.startTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.startTime.Value:HH:mm:ss}" : null,
			////    End = s.scheduleDate != null && s.endTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.endTime.Value:HH:mm:ss}" : null,
			////    Url = "/dentist/dentistdetail/patientappointments?appointmentID=" + s.appointmentID,
			////    StatusColor = s.appointmentStatus switch
			////    {
			////        "Chờ Xác Nhận" => "#d5c700", // Yellow
			////        "Đã Chấp Nhận" => "#0078d5", // Blue
			////        "Đã Hủy" => "#d53700", // Red
			////        "Đã Khám" => "#00d55f", // Green
			////        _ => "#c2c2c2" // Default color (Grey) nếu không trùng với mấy cái trên
			////    }
			////}).ToList();
			//#endregion
			//// Lấy Schedule của dentist cụ thể
			//var schedules = await _context.Schedules
			//                        .Include(s => s.Dentist)
			//                        .Include(s => s.TimeSlot)
			//                        .Where(s => s.Dentist.Account.ID == dentistAccountID)
			//                        .ToListAsync();

			//// Map 2 bên Schedule với appointment, sau đó map những cột appointment ko null với patient record
			//var scheduleAppointments = from schedule in schedules
			//                           join appointment in _context.Appointments
			//                           on schedule.ID equals appointment.ScheduleID into appointmentGroup
			//                           from appointment in appointmentGroup.DefaultIfEmpty()
			//                           select new
			//                           {
			//                               appointmentStatus = appointment?.AppointmentStatus ?? "",
			//                               appointmentID = appointment?.ID ?? 0,
			//                               scheduleDate = schedule.Date,
			//                               patientID = appointment?.PatientRecordID ?? 0,
			//                               startTime = schedule?.TimeSlot?.StartTime,
			//                               endTime = schedule?.TimeSlot?.EndTime,
			//                               patientRecordID = appointment?.PatientRecordID ?? 0
			//                           };

			//var result = from sa in scheduleAppointments
			//             join patientRecord in _context.PatientRecords
			//             on sa.patientRecordID equals patientRecord.ID into patientGroup
			//             from patientRecord in patientGroup.DefaultIfEmpty()
			//             select new
			//             {
			//                 sa.appointmentStatus,
			//                 sa.appointmentID,
			//                 sa.scheduleDate,
			//                 sa.startTime,
			//                 sa.endTime,
			//                 patientName = patientRecord?.FullName ?? "No Patient"
			//             };

			//var events = new List<EventVM>();

			//foreach (var schedule in schedules)
			//{
			//    if (schedule.TimeSlot?.StartTime == null || schedule.TimeSlot?.EndTime == null)
			//    {
			//        continue;
			//    }

			//    var startTime = schedule.TimeSlot.StartTime;
			//    var endTime = schedule.TimeSlot.EndTime;

			//    var currentSlotStart = startTime;
			//    while (currentSlotStart < endTime)
			//    {
			//        var currentSlotEnd = currentSlotStart.Add(TimeSpan.FromMinutes(30));
			//        if (currentSlotEnd > endTime)
			//        {
			//            break;
			//        }

			//        var slotExists = result.Any(r => r.scheduleDate == schedule.Date &&
			//                                         r.startTime == currentSlotStart &&
			//                                         r.endTime == currentSlotEnd);

			//        if (!slotExists)
			//        {
			//            events.Add(new EventVM
			//            {
			//                Title = "Trống",
			//                Start = $"{schedule.Date:yyyy-MM-dd}T{currentSlotStart:HH:mm:ss}",
			//                End = $"{schedule.Date:yyyy-MM-dd}T{currentSlotEnd:HH:mm:ss}",
			//                Url = "/dentist/dentistdetail/patientappointments?appointmentID=0",
			//                StatusColor = "#c2c2c2" // Default color (Grey)
			//            });
			//        }

			//        currentSlotStart = currentSlotEnd;
			//    }
			//}

			//// Add only the appointments to the events list, not the original time slots
			//events.AddRange(result.Select(s => new EventVM
			//{
			//    Title = s.appointmentID != 0 ? $"#{s.appointmentID} - {s.patientName}" : "Trống",
			//    Start = s.scheduleDate != null && s.startTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.startTime.Value:HH:mm:ss}" : null,
			//    End = s.scheduleDate != null && s.endTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.endTime.Value:HH:mm:ss}" : null,
			//    Url = "/dentist/dentistdetail/patientappointments?appointmentID=" + s.appointmentID,
			//    StatusColor = s.appointmentStatus switch
			//    {
			//        "Chờ Xác Nhận" => "#d5c700", // Yellow
			//        "Đã Chấp Nhận" => "#0078d5", // Blue
			//        "Đã Hủy" => "#d53700", // Red
			//        "Đã Khám" => "#00d55f", // Green
			//        _ => "#c2c2c2" // Default color (Grey) nếu không trùng với mấy cái trên
			//    }
			//}).ToList());
			#endregion

			if (dentist == null)
			{
				return NotFound("Không tìm thấy nha sĩ này");
			}

			// Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == dentist.Clinic.ID);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);

			DateTime utc7Now = Util.GetUtcPlus7Time().AddHours(12); // Thêm thời gian 12 tiếng cho phù hợp với business rule
			DateOnly todayDate = DateOnly.FromDateTime(utc7Now);
			TimeOnly todayTime = TimeOnly.FromDateTime(utc7Now);

			var schedules = await _context.Schedules
				.Include(s => s.Dentist)
				.Include(s => s.TimeSlot)
				.Include(s => s.Appointments)
				.Where(s => s.DentistID == dentist.ID) // && s.ScheduleStatus == "Còn Trống"
													   //(s.Date > todayDate || (s.Date >= todayDate && s.TimeSlot.StartTime >= todayTime)) &&
				.ToListAsync();

			// lấy tất cả các ngày có trong schedules
			var allSchedules = schedules.Select(s => new
			{
				TimeSlotId = s.TimeSlot.ID,
				Date = s.Date
			}).Distinct().ToList();

			// tạo danh sách các time slot với thời gian 30 phút cho từng ngày
			var timeSlots = new List<object>();
			var appointments = _context.Appointments
				.Include(a => a.PatientRecords)
				.Where(a => a.Schedule.DentistID == dentist.ID)
				.ToList();

			// chuyển đổi danh sách các cuộc hẹn thành dictionary để search
			var appointmentDict = appointments
				.GroupBy(a => new { a.Schedule.Date, a.Schedule.TimeSlot.StartTime })
				.ToDictionary(g => g.Key
				);

			// Lấy danh sách các lịch đièu trị (future appoint...)
			var futureAppointments = _context.FutureAppointments
				.Include(f => f.PatientRecord)
				.Where(f => f.Dentist_ID == dentist.ID)
				.ToList();

			// chuyển đổi danh sách lịch điều trị thành dictionary
			var futureAppointmentDict = futureAppointments
				.GroupBy(f => new { Date = f.DesiredDate, StartTime = f.StartTime })
				.ToDictionary(g => g.Key
				);

			DateOnly maxDate = new DateOnly(1900, 1, 1);

			foreach (var schedule in allSchedules)
			{
				var dailyTimeSlots = new List<object>();
				List<TimeSlot> availableTimeSlot = new();

				//Lấy ngày lớn nhất trog d.sách schedule
				if (maxDate < schedule.Date)
				{
					maxDate = schedule.Date;
				}

				if (schedule.TimeSlotId == 1)
				{
					availableTimeSlot = amTimeSlots;
				}
				else if (schedule.TimeSlotId == 2)
				{
					availableTimeSlot = pmTimeSlots;
				}
				else
				{
					continue;
				}

				foreach (var slot in availableTimeSlot)
				{
					var startTime = slot.StartTime;
					var endTime = slot.EndTime;
					while (startTime < endTime)
					{
						var nextTime = startTime.AddMinutes(30);
						if (nextTime > endTime) nextTime = endTime;

						// Kiểm tra xem có cuộc hẹn nào trong khung thời gian này không
						var key = new { Date = schedule.Date, StartTime = startTime };


						if (appointmentDict.ContainsKey(key) && appointmentDict.TryGetValue(key, out var values))
						{
							foreach (var value in values)
							{
								dailyTimeSlots.Add(new EventVM
								{
									Title = $"#{value.ID} {value.PatientRecords.FullName}",
									Start = $"{schedule.Date:yyyy-MM-dd}T{startTime:HH:mm:ss}",
									End = $"{schedule.Date:yyyy-MM-dd}T{nextTime:HH:mm:ss}",
									Url = "/dentist/dentistdetail/patientappointments?appointmentID=" + value.ID,
									StatusColor = value.AppointmentStatus switch
									{
										"Chờ Xác Nhận" => "#d5c700", // Yellow
										"Đã Chấp Nhận" => "#0078d5", // Blue
										"Đã Hủy" => "#d53700", // Red
										"Đã Khám" => "#00d55f", // Green
										_ => "#c2c2c2" // Default color (Grey) nếu không trùng với mấy cái trên
									}
								});
							}

						}

						else if (futureAppointmentDict.ContainsKey(key) && futureAppointmentDict.TryGetValue(key, out var futureAppointmentValues))
						{
							foreach (var futureAppointment in futureAppointmentValues)
							{
								dailyTimeSlots.Add(new EventVM
								{
									Title = $"ĐIỀU TRỊ - {futureAppointment.PatientRecord.FullName}",
									Start = $"{schedule.Date:yyyy-MM-dd}T{startTime:HH:mm:ss}",
									End = $"{schedule.Date:yyyy-MM-dd}T{nextTime:HH:mm:ss}",
									Url = "/dentist/dentistdetail/showperiodicappointment?periodicappointmentID=" + futureAppointment.ID,
									StatusColor = "aqua"
								});
							}
						}

						else// if(!appointmentDict.ContainsKey(key))
						{
							dailyTimeSlots.Add(new EventVM
							{
								Title = "Trống",
								Start = $"{schedule.Date:yyyy-MM-dd}T{startTime:HH:mm:ss}",
								End = $"{schedule.Date:yyyy-MM-dd}T{nextTime:HH:mm:ss}",
								Url = "/dentist/dentistdetail/patientappointments?appointmentID=0",
								StatusColor = "#c2c2c2"
							});
						}

						startTime = nextTime;
					}
				}

				// Add dailyTimeSlots vào timeSlots chung
				timeSlots.AddRange(dailyTimeSlots);

			}

			Schedule lastSchedule = schedules.Find(ls => ls.Date == maxDate);
			Console.WriteLine(lastSchedule.Date);
			foreach (var futureAppoint in futureAppointments)
			{


				if (futureAppoint.DesiredDate > lastSchedule.Date)
				{
					timeSlots.Add(new EventVM
					{
						Title = $"ĐIỀU TRỊ - {futureAppoint.PatientRecord.FullName}",
						Start = $"{futureAppoint.DesiredDate:yyyy-MM-dd}T{futureAppoint.StartTime:HH:mm:ss}",
						End = $"{futureAppoint.DesiredDate:yyyy-MM-dd}T{futureAppoint.EndTime:HH:mm:ss}",
						Url = "/dentist/dentistdetail/showperiodicappointment?periodicappointmentID=" + futureAppoint.ID,
						StatusColor = "aqua"
					});
				}
			}



			// Gửi thông tin qua View
			ViewBag.dentistAvatar = dentist?.Account.Image;
			ViewBag.dentistName = $"{dentist?.Account.LastName} {dentist?.Account.FirstName}";
			ViewBag.events = JsonConvert.SerializeObject(timeSlots);
			TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
			return View();
		}

		#endregion

		#region Lấy thông tin về những ngày mà dentist đó có lịch hoặc có hẹn 
		public async Task<IActionResult> GetScheduleDates(int? dentistID)
		{
			var dentist = await _context.Dentists.Include(d => d.Clinic).Where(d => d.ID == dentistID).FirstAsync();

			

			List<object> timeSlot = new();
			foreach(var slot in GenerateTimeSlots(dentist.Clinic.AmWorkTimeID))
			{
				timeSlot.Add( new
				{
					Start = $"{slot.StartTime:HH:mm}",
					End = $"{slot.EndTime:HH:mm}",
					Title = "AM"
				});
			}
			foreach (var slot in GenerateTimeSlots(dentist.Clinic.PmWorkTimeID))
			{
				timeSlot.Add( new
				{
					Start = $"{slot.StartTime:HH:mm}",
					End = $"{slot.EndTime:HH:mm}",
					Title = "PM"
				});
			}

			var result = new
			{
				TimeSlot = timeSlot
			};

			return Json(result);
		}
		#endregion

		#region Chỉnh sửa thông tin của Dentist

		[HttpGet]
		public async Task<IActionResult> DentistDescription()
		{
			var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
			if (dentistAccountID == null)
			{
				return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
			}
			var dentist = await _context.Dentists.Where(d => d.Account.ID == dentistAccountID).Include(d => d.Account).FirstAsync();
			ViewBag.DentistAvatar = dentist?.Account.Image ?? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FPatient%2Fuser.png?alt=media&token=9010a4a6-0220-4d29-bb85-1fe425100744";
			ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
			//Lấy ra những thông báo cần hiển thị
			TempData["ErrorMessage"] = TempData["ErrorMessage"] as string;
			TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
			return View(dentist);
		}





		[HttpPost]
		public async Task<IActionResult> SendDentistDescription(string? content)
		{
			var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
			if (dentistAccountID == null)
			{
				return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
			}
			if (string.IsNullOrEmpty(content))
			{
				TempData["ErrorMessage"] = "Lỗi! Mô tả không được để trống.";
				return View();
			}
			var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.AccountID == dentistAccountID);
			dentist.Description = content;
			_context.Dentists.Update(dentist);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Thay đổi thành công!";

			return View();
		}

		#endregion

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

		//================ FUTURE APPOINTMENT =================

		#region Hàm lấy thông tin của tất cả lịch điều trị cho nha sĩ đó - FUTURE APPOINTMENT
		[HttpGet]
		[Route("periodicappointment")]
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

		#region helper method. Author: Ngoc Anh
		private List<TimeSlot> GenerateTimeSlots(int workTimeId)
		{

			// Retrieve the WorkTime based on the given ID
			var workTime = _context.WorkTimes
								  .FirstOrDefault(wt => wt.ID == workTimeId);

			if (workTime == null)
			{
				Console.WriteLine("WorkTime not found.");
				return new List<TimeSlot>();
			}

			var startTime = workTime.StartTime;
			var endTime = workTime.EndTime;

			// Retrieve the matching TimeSlots
			return _context.TimeSlots
						  .Where(ts => ts.StartTime >= startTime && ts.EndTime <= endTime).ToList();

		}
		#endregion
	}
}
