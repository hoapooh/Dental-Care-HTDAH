using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Google.Apis.PeopleService.v1.Data;
using Dental_Clinic_System.ViewModels;
using Newtonsoft.Json;
using System.Collections;
using Microsoft.AspNetCore.Authorization;

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
	[Area("Manager")]
	//[Route("Manager/[controller]/[action]")]
	[Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
	public class SchedulesController : Controller
	{
		private readonly DentalClinicDbContext _context;
		public SchedulesController(DentalClinicDbContext context)
		{
			_context = context;
		}
		#region Bảng lịch đã đặt -> xem trạng thái đơn khám
		// GET: Manager/Schedules
		public async Task<IActionResult> GetBookedSchedule(string type, int? dentistId, DateTime? date, string status)
		{
			// Set ViewBag values to retain search criteria
			ViewBag.SelectedType = type;
			ViewBag.SelectedDentistId = dentistId;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd"); // Format the date for the input
            ViewBag.SelectedStatus = status;
			//--------------------------------------------
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			//---------------------------------------------------
			//Lấy các lịch trong Appointment Table
			// Filter schedules from today onwards
			var today = DateOnly.FromDateTime(DateTime.Today);
			var sche_Appointment = _context.Appointments.Include(a => a.Schedule).ThenInclude(s => s.Dentist).ThenInclude(d => d.Account).Include(a => a.Schedule).ThenInclude(s => s.TimeSlot).Where(a => a.Schedule.Dentist.ClinicID == clinicId && a.Schedule.Date >= today);
			var sche_FutureAppts = _context.PeriodicAppointments.Include(f => f.Dentist).Where(f => f.Dentist.ClinicID == clinicId && f.DesiredDate >= today);
            //--------------------------------------------------
            ViewBag.Dentists = await _context.Dentists.Include(d => d.Account).Where(d => d.ClinicID == clinicId).ToListAsync();
			if (type == "Tái Khám / Điều Trị")
			{
				ViewBag.Status = new List<string>() { "Đã Chấp Nhận", "Đã Khám", "Đã Hủy" };
			} 
			else
			{
				ViewBag.Status = new List<string>() { "Chờ Xác Nhận", "Đã Chấp Nhận", "Đã Khám", "Đã Hủy" };
			}
			//--------------------------------------------------
			
            if (dentistId.HasValue)
            {
				sche_Appointment = sche_Appointment.Where(s => s.Schedule.DentistID == dentistId);
				sche_FutureAppts = sche_FutureAppts.Where(s => s.Dentist_ID == dentistId);
            }
            if (date.HasValue)
            {
                sche_Appointment = sche_Appointment.Where(s => s.Schedule.Date == DateOnly.FromDateTime(date.Value));
                sche_FutureAppts = sche_FutureAppts.Where(s => s.DesiredDate == DateOnly.FromDateTime(date.Value));
            }
            if (!string.IsNullOrEmpty(status))
            {
                sche_Appointment = sche_Appointment.Where(s => s.AppointmentStatus == status);
                sche_FutureAppts = sche_FutureAppts.Where(s => s.PeriodicAppointmentStatus == status);
            }
			//------------------------------------------------------
            var bookedSchedules = new List<BookedScheduleVM>();
			if (type == null || type == "Khám")
			{
				foreach (var sche in sche_Appointment.ToList())
				{
					bookedSchedules.Add(new()
					{
						DentistName = sche.Schedule.Dentist.Account.LastName + " " + sche.Schedule.Dentist.Account.FirstName,
						Date = sche.Schedule.Date,
						Start_EndTime = sche.Schedule.TimeSlot.StartTime.ToString("HH:mm") + " - " + sche.Schedule.TimeSlot.EndTime.ToString("HH:mm"),
						Type = "Khám",
						Status = sche.AppointmentStatus,
						Description = sche.Description
					});
				}
			}
			if (type == null || type == "Tái Khám / Điều Trị")
			{
				foreach (var sche in sche_FutureAppts.ToList())
				{
					bookedSchedules.Add(new()
					{
						DentistName = sche.Dentist.Account.LastName + " " + sche.Dentist.Account.FirstName,
						Date = sche.DesiredDate,
						Start_EndTime = sche.StartTime.ToString("HH:mm") + " - " + sche.EndTime.ToString("HH:mm"),
						Type = "Tái Khám / Điều Trị",
						Status = sche.PeriodicAppointmentStatus,
						Description = sche.Description
					});
				}
			}
			return View("GetBookedSchedule", bookedSchedules.OrderBy(b => b.Date).ThenBy(b => b.Start_EndTime).ToList());
		}
		public async Task<IActionResult> Index(int? dentistId, DateTime? date, string status)
		{
			// Set ViewBag values to retain search criteria
			ViewBag.SelectedDentistId = dentistId;
			ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd"); // Format the date for the input
			ViewBag.SelectedStatus = status;
			//--------------------------------------------
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			//---------------------------------------------------
			//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == clinicId);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
			ViewBag.AmTimeSlots = amTimeSlots;
			ViewBag.PmTimeSlots = pmTimeSlots;

			List<TimeSlot> timeSlots = GenerateTimeSlots(new TimeOnly(7, 0), new TimeOnly(21, 0));
			ViewBag.TimeSlots = timeSlots;
			//--------------------------------------------------
			ViewBag.Dentists = await _context.Dentists.Include(d => d.Account).Where(d => d.ClinicID == clinicId).ToListAsync();
			ViewBag.Status = null;

			//--------------------------------------------------
			// Filter schedules from today onwards
			var today = DateOnly.FromDateTime(DateTime.Today);
			var schedulesQuery = _context.Schedules
					.Include(s => s.Dentist)
					.ThenInclude(d => d.Account)
					.Include(s => s.TimeSlot)
					.Where(s => s.Dentist.ClinicID == clinicId && s.Date >= today);
			if (dentistId.HasValue)
			{
				schedulesQuery = schedulesQuery.Where(s => s.DentistID == dentistId.Value);
			}

			if (date.HasValue)
			{
				schedulesQuery = schedulesQuery.Where(s => s.Date == DateOnly.FromDateTime(date.Value));
			}

			if (!string.IsNullOrEmpty(status))
			{
				if (status != "Còn Trống")
					schedulesQuery = schedulesQuery.Where(s => s.ScheduleStatus == status);
				ViewBag.Status = status;
			}

			var dentalClinicDbContext = await schedulesQuery.OrderBy(s => s.Date).ToListAsync();
			//--------------------------
			//Lấy các lịch điều trị/ Lịch tái khám - BẢNG FUTUREAPPOINTMENT
			var futureListDb = await _context.PeriodicAppointments.Include(f => f.Dentist).ThenInclude(d => d.Account).Where(f => f.PeriodicAppointmentStatus != "Đã Hủy").OrderBy(f => f.DesiredDate).ToListAsync();
			List<FutureAppointmentVM> futureList = new();
            foreach (var sche in futureListDb)
            {
				futureList.Add(new()
				{
					DentistID = sche.Dentist_ID,
					Date = sche.DesiredDate,
					Slots = GenerateTimeSlotIDs(sche.StartTime, sche.EndTime)
				});

			}
			ViewBag.FutureList = futureList;
			return View("Index", dentalClinicDbContext);
		}
		public async Task<IActionResult> ViewHistory(string type, int? dentistId, DateTime? date, string status)
		{
			// Set ViewBag values to retain search criteria
			ViewBag.SelectedType = type;
			ViewBag.SelectedDentistId = dentistId;
			ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd"); // Format the date for the input
			ViewBag.SelectedStatus = status;
			//--------------------------------------------
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			//---------------------------------------------------
			//Lấy các lịch trong Appointment Table
			// Filter schedules from today view history
			var today = DateOnly.FromDateTime(DateTime.Today);
			var sche_Appointment = _context.Appointments.Include(a => a.Schedule).ThenInclude(s => s.Dentist).ThenInclude(d => d.Account).Include(a => a.Schedule).ThenInclude(s => s.TimeSlot).Where(a => a.Schedule.Dentist.ClinicID == clinicId && a.Schedule.Date < today);
			var sche_FutureAppts = _context.PeriodicAppointments.Include(f => f.Dentist).Where(f => f.Dentist.ClinicID == clinicId && f.DesiredDate < today);
			//--------------------------------------------------
			ViewBag.Dentists = await _context.Dentists.Include(d => d.Account).Where(d => d.ClinicID == clinicId).ToListAsync();
			if (type == "Tái Khám / Điều Trị")
			{
				ViewBag.Status = new List<string>() { "Đã Chấp Nhận", "Đã Khám", "Đã Hủy", "Chưa Khám" };
			}
			else
			{
				ViewBag.Status = new List<string>() { "Chờ Xác Nhận", "Đã Chấp Nhận", "Đã Khám", "Đã Hủy", "Chưa Khám" };
			}
			//--------------------------------------------------
			
			if (dentistId.HasValue)
			{
				sche_Appointment = sche_Appointment.Where(s => s.Schedule.DentistID == dentistId);
				sche_FutureAppts = sche_FutureAppts.Where(s => s.Dentist_ID == dentistId);
			}
			if (date.HasValue)
			{
				sche_Appointment = sche_Appointment.Where(s => s.Schedule.Date == DateOnly.FromDateTime(date.Value));
				sche_FutureAppts = sche_FutureAppts.Where(s => s.DesiredDate == DateOnly.FromDateTime(date.Value));
			}
			if (!string.IsNullOrEmpty(status))
			{
				sche_Appointment = sche_Appointment.Where(s => s.AppointmentStatus == status);
				sche_FutureAppts = sche_FutureAppts.Where(s => s.PeriodicAppointmentStatus == status);
			}
			//------------------------------------------------------
			var bookedSchedules = new List<BookedScheduleVM>();
			if (type == null || type == "Khám")
			{
				foreach (var sche in sche_Appointment.ToList())
				{
					bookedSchedules.Add(new()
					{
						DentistName = sche.Schedule.Dentist.Account.LastName + " " + sche.Schedule.Dentist.Account.FirstName,
						Date = sche.Schedule.Date,
						Start_EndTime = sche.Schedule.TimeSlot.StartTime.ToString("HH:mm") + " - " + sche.Schedule.TimeSlot.EndTime.ToString("HH:mm"),
						Type = "Khám",
						Status = sche.AppointmentStatus,
						Description = sche.Description
					});
				}
			}
			if (type == null || type == "Tái Khám / Điều Trị")
			{
				foreach (var sche in sche_FutureAppts.ToList())
				{
					bookedSchedules.Add(new()
					{
						DentistName = sche.Dentist.Account.LastName + " " + sche.Dentist.Account.FirstName,
						Date = sche.DesiredDate,
						Start_EndTime = sche.StartTime.ToString("HH:mm") + " - " + sche.EndTime.ToString("HH:mm"),
						Type = "Tái Khám / Điều Trị",
						Status = sche.PeriodicAppointmentStatus,
						Description = sche.Description
					});
				}
			}
			return View("ViewHistory", bookedSchedules.OrderBy(b => b.Date).ToList());
		}
		private List<int> GenerateTimeSlotIDs(TimeOnly startTime, TimeOnly endTime)
        {
            // Retrieve the matching TimeSlots
            return _context.TimeSlots
                          .Where(ts => ts.StartTime >= startTime && ts.EndTime <= endTime && ts.ID != 1 && ts.ID != 2).Select(ts => ts.ID).ToList();

        }
        private List<TimeSlot> GenerateTimeSlots(TimeOnly startTime, TimeOnly endTime)
        {
            // Retrieve the matching TimeSlots
            return _context.TimeSlots
                          .Where(ts => ts.StartTime >= startTime && ts.EndTime <= endTime && ts.ID != 1 && ts.ID != 2).ToList();

        }
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
						  .Where(ts => ts.StartTime >= startTime && ts.EndTime <= endTime && ts.ID != 1 && ts.ID != 2).ToList();

		}
		#endregion

		#region Lich làm việc của Nha Sĩ - Hàng tuần - Lịch này hỗ trợ generatr lịch khám theo tuần nhanh chóng
		// GET: Manager/LichLamViec
		public async Task<IActionResult> GetWorkingSchedule()
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			//Lịch làm việc sáng chiều của phòng khám.
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == clinicId);
			ViewBag.SangStart = clinic.AmWorkTimes.StartTime;
			ViewBag.SangEnd = clinic.AmWorkTimes.EndTime;
			ViewBag.ChieuStart = clinic.PmWorkTimes.StartTime;
			ViewBag.ChieuEnd = clinic.PmWorkTimes.EndTime;

			//--------------------------------------------
			// Lấy các lịch làm việc của các nha sĩ thuộc phòng khám cụ thể - mà manager đang quản lý
			//Lấy tất cả các dòng của Dentist_Sessions where clinicID và Dentist "Hoạt Động"
			ViewBag.DenSesList = await _context.Dentist_Sessions.Include(d => d.Dentist).ThenInclude(a => a.Account).Where(p => p.Dentist.ClinicID == clinicId && p.Dentist.Account.AccountStatus == "Hoạt Động").ToListAsync(); 
			return View();
		}
		// POST: Manager/LichLamViec
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditWorkingSchedule(List<int> SelectedDenSesList)
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			// Lấy các lịch làm việc của các nha sĩ thuộc phòng khám cụ thể - mà manager đang quản lý
			//Lấy tất cả các dòng của Dentist_Sessions where clinicID và Dentist "Hoạt Động"
			var den_sesList = await _context.Dentist_Sessions.Include(d => d.Dentist).ThenInclude(a => a.Account).Where(p => p.Dentist.ClinicID == clinicId && p.Dentist.Account.AccountStatus == "Hoạt Động").ToListAsync();
			foreach (var denses in den_sesList)
			{
				if (SelectedDenSesList.Contains(denses.ID)) //nếu được chọn
					denses.Check = true;
				else
					denses.Check = false;
				_context.Update(denses);
			}
			await _context.SaveChangesAsync();
            return RedirectToAction("GetWorkingSchedule");
        }
		#endregion

		#region Tạo lịch theo tuần - dựa trên lịch hàng tuần (quy định sáng, chiều có làm việc ko)
		// POST: Manager/LichLamViec
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateWeekSchedule(string selectedDates)
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			// Check if there are any active dentists in the clinic
			var activeDentists = await _context.Dentists.Include(d => d.Account)
				.Where(d => d.ClinicID == clinicId && d.Account.AccountStatus == "Hoạt Động")
				.ToListAsync();

			if (!activeDentists.Any())
			{
				TempData["ToastMessageFailTempData"] = "Phòng khám hiện không có nha sĩ nào làm việc.";
				return RedirectToAction("GetWorkingSchedule");
			}


			List<DateOnly> dates = selectedDates.Split(',')
                    .Select(date => DateOnly.ParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture))
                    .ToList();
            if (!string.IsNullOrEmpty(selectedDates))
			{
				// Tạo dictionary để dễ dàng map session ID với ngày và time slot
				var sessionToDayTime = new Dictionary<int, (int, int)>
				{
					{1, (0, 1)}, {2, (0, 2)}, {3, (1, 1)}, {4, (1, 2)},
					{5, (2, 1)}, {6, (2, 2)}, {7, (3, 1)}, {8, (3, 2)},
					{9, (4, 1)}, {10, (4, 2)}, {11, (5, 1)}, {12, (5, 2)},
					{13, (6, 1)}, {14, (6, 2)}
				}; //Dictionary là một cấu trúc dữ liệu trong C# dùng để lưu trữ các cặp khóa-giá trị (key-value pairs). Trong trường hợp này, sessionToDayTime là một dictionary với session_ID là khóa và cặp (dayIndex, timeSlotId) là giá trị.
				var dentistSessions = await _context.Dentist_Sessions
					.Include(ds => ds.Dentist)
					.ThenInclude(d => d.Account)
					.Where(ds => ds.Dentist.ClinicID == clinicId && ds.Check == true && ds.Dentist.Account.AccountStatus == "Hoạt Động")
					.ToListAsync();
				var newScheList = new List<Schedule>();
				foreach (var group in dentistSessions.GroupBy(ds => ds.Dentist_ID))
				{
					foreach (var ds in group)
					{
						if (sessionToDayTime.TryGetValue(ds.Session_ID, out var dayTime))
						//Phương thức TryGetValue của Dictionary giúp kiểm tra xem một khóa cụ thể có tồn tại trong dictionary hay không. Nếu tồn tại, nó sẽ trả về giá trị tương ứng và đặt vào biến out.
						//Cú pháp: dictionary.TryGetValue(key, out value);

						{
							var (dayIndex, timeSlotId) = dayTime;
							var date = dates[dayIndex];

							if (!_context.Schedules.Any(s => s.DentistID == ds.Dentist_ID && s.Date == date && s.TimeSlotID == timeSlotId))
							{
								newScheList.Add(new Schedule
								{
									DentistID = ds.Dentist_ID,
									Date = date,
									TimeSlotID = timeSlotId,
									ScheduleStatus = (timeSlotId == 1) ? "Lịch Sáng" : "Lịch Chiều"
								});
							}
						}
					}
				}
				_context.AddRange(newScheList);
				await _context.SaveChangesAsync();
			}

			var today = DateOnly.FromDateTime(DateTime.Today);
			if (today < dates[0])
			{
				TempData["ToastMessageSuccessTempData"] = "Thành công tạo lịch khám từ ngày " + dates[0].ToString("dd/MM/yyyy") + " đến " + dates[6].ToString("dd/MM/yyyy");
			}
			else
			{
				TempData["ToastMessageSuccessTempData"] = "Thành công tạo lịch khám từ ngày " + today.ToString("dd/MM/yyyy") + " đến " + dates[6].ToString("dd/MM/yyyy");
			}
			
            return RedirectToAction("GetWorkingSchedule");
		}
		#endregion

		#region Lấy lịch làm việc của Dentist cụ thể, đưa vào Calendar - ORIGINAL AUTHOR: PHẠM DUY HOÀNG
		[HttpGet]
		//[Authorize(Roles = "Nha Sĩ")]
		public async Task<IActionResult> DentistSchedule()
		{

			var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
			if (dentistAccountID == null)
			{
				return RedirectToAction("Login", "DentistAccount", new { area = "Dentist" });
			}

			// Lấy Schedule của dentist cụ thể
			var schedules = await _context.Schedules
									.Include(s => s.Dentist)
									.Include(s => s.TimeSlot)
									.Where(s => s.Dentist.Account.ID == dentistAccountID)
									.ToListAsync();

			if (schedules == null || !schedules.Any())
			{
				ViewBag.message = "Không tìm thấy lịch phù hợp";
				return View();
			}

			// Map 2 bên Schedule với appointment, sau đó map những cột appointment ko null với patient record
			var scheduleAppointments = from schedule in schedules
									   join appointment in _context.Appointments
									   on schedule.ID equals appointment.ScheduleID into appointmentGroup
									   from appointment in appointmentGroup.DefaultIfEmpty()
									   select new
									   {
										   appointmentID = appointment?.ID ?? 0,
										   scheduleDate = schedule.Date,
										   patientID = appointment?.PatientRecordID ?? 0,
										   startTime = schedule?.TimeSlot?.StartTime,
										   endTime = schedule?.TimeSlot?.EndTime,
										   patientRecordID = appointment?.PatientRecordID ?? 0
									   };


			var result = from sa in scheduleAppointments
						 join patientRecord in _context.PatientRecords
						 on sa.patientRecordID equals patientRecord.ID into patientGroup
						 from patientRecord in patientGroup.DefaultIfEmpty()
						 select new
						 {
							 sa.appointmentID,
							 sa.scheduleDate,
							 sa.startTime,
							 sa.endTime,
							 patientName = patientRecord?.FullName ?? "No Patient"
						 };

			// Map to EventVM
			var events = result.Select(s => new EventVM
			{
				Title = s.appointmentID != 0 ? $"#{s.appointmentID} - {s.patientName}" : "Trống",
				Start = s.scheduleDate != null && s.startTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.startTime.Value:HH:mm:ss}" : null,
				End = s.scheduleDate != null && s.endTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.endTime.Value:HH:mm:ss}" : null,
			}).ToList();


			// Lấy thông tin Nha sĩ
			var dentist = await _context.Dentists
										.Include(d => d.Account)
										.FirstOrDefaultAsync(d => d.Account.ID == dentistAccountID);

			// Gửi thông tin qua View
			ViewBag.dentistAvatar = dentist.Account.Image;
			ViewBag.dentistName = $"{dentist.Account.LastName} {dentist.Account.FirstName}";
			ViewBag.events = JsonConvert.SerializeObject(events);
			return View();
		}

		#endregion

		#region Tạo lịch khám => Chọn nhiều nha sĩ, chọn nhiều ngày, chọn nhiều timeslot => Nhiều lịch khám
		// GET: Manager/Schedules/Create
		public IActionResult Create()
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			//---------------------------------------------------
			//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic =  _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefault(m => m.ID == clinicId);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
			ViewBag.AmTimeSlots = amTimeSlots;
			ViewBag.PmTimeSlots = pmTimeSlots;

			//--------------------------------------------------
			var dentists = _context.Dentists
						   .Join(_context.Accounts,
								 dentist => dentist.AccountID,
								 account => account.ID,
								 (dentist, account) => new
								 {
									 DentistID = dentist.ID,
									 FullName = account.LastName + " " + account.FirstName,
									 ClinicID = dentist.ClinicID,
									 Status = account.AccountStatus
								 })
						   .ToList();

			ViewData["DentistID"] = new SelectList(dentists.Where(d => d.ClinicID == clinicId && d.Status == "Hoạt Động"), "DentistID", "FullName");
			return View(new ScheduleVM());
		}

		// POST: Manager/Schedules/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("DentistIDs, Dates, TimeSlots")] ScheduleVM schedule)
		{
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var dentists = _context.Dentists
                           .Join(_context.Accounts,
                                 dentist => dentist.AccountID,
                                 account => account.ID,
                                 (dentist, account) => new
                                 {
                                     DentistID = dentist.ID,
                                     FullName = account.LastName + " " + account.FirstName,
                                     ClinicID = dentist.ClinicID,
                                     Status = account.AccountStatus
                                 })
                           .ToList();

            ViewData["DentistID"] = new SelectList(dentists.Where(d => d.ClinicID == clinicId && d.Status == "Hoạt Động"), "DentistID", "FullName");
			//---------------------------------------------------
			//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic = _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefault(m => m.ID == clinicId);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
			ViewBag.AmTimeSlots = amTimeSlots;
			ViewBag.PmTimeSlots = pmTimeSlots;

			//-----------------------------------------------------------------------------
			if (schedule.DentistIDs.Count == 0)
			{
				TempData["ToastMessageFailTempData"] = "Bạn chưa chọn nha sĩ nào.";
                return View(schedule);
            } 
			if (schedule.Dates == null)
			{
                TempData["ToastMessageFailTempData"] = "Bạn chưa chọn ngày nào.";
                return View(schedule);
            }
            if (schedule.TimeSlots.Count == 0)
            {
                TempData["ToastMessageFailTempData"] = "Bạn chưa chọn khung giờ nào.";
                return View(schedule);
            }
            if (schedule.DentistIDs.Count > 0 && schedule.Dates != null && schedule.TimeSlots.Count > 0)
			{
				var newScheList = new List<Schedule>();
				List<DateOnly> dateList = ConvertStringToDateOnlyList(schedule.Dates);
				foreach (var dentist in schedule.DentistIDs)
				{
					foreach (var date in dateList)
					{
						foreach (var slot in schedule.TimeSlots)
						{
							var existSchedule = await _context.Schedules.FirstOrDefaultAsync(
								a => a.DentistID == dentist && a.Date == date && a.TimeSlotID == slot && a.ScheduleStatus != "Đã Hủy");

							if (existSchedule == null)
							{
								newScheList.Add( new Schedule
								{
									//DentistID = schedule.DentistIDs,
									DentistID = dentist,
									Date = date,
									TimeSlotID = slot,
									ScheduleStatus = "Còn Trống"
								});
							}
						}
					}
				}
				//------------
				_context.AddRange(newScheList);
				await _context.SaveChangesAsync();
				TempData["ToastMessageSuccessTempData"] = "Thành công tạo lịch khám.";
                return RedirectToAction(nameof(Index));
			}
			return View(schedule);
		}
		private List<DateOnly> ConvertStringToDateOnlyList(string dateString)
		{
			List<DateOnly> dateList = new List<DateOnly>();
			string[] dateArray = dateString.Split(new[] { ", " }, StringSplitOptions.None);

			foreach (string date in dateArray)
			{
				DateOnly parsedDate = DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
				dateList.Add(parsedDate);
			}

			return dateList;
		}
		#endregion

		#region Sửa lịch khám - Vô bảng riêng của "Nha Sĩ - Ngày" cụ thể 
		// GET: Manager/Schedules/...
		public async Task<IActionResult> Edit(int? dentistId, DateTime? date)
		{
			if (dentistId == null || date == null)
			{
				return NotFound();
			}
			
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			//--------------------------------------------
			var scheduleSubList = _context.Schedules.Include(s => s.Dentist).ThenInclude(d => d.Account).Include(s => s.TimeSlot).Where(p =>
				p.DentistID == dentistId && p.Date == DateOnly.FromDateTime(date.Value));

			var dentist = await _context.Dentists.Include(d => d.Account).FirstOrDefaultAsync(m => m.ID == dentistId);
			ViewBag.DentistName = dentist.Account.LastName + " " + dentist.Account.FirstName;
			ViewBag.Date = DateOnly.FromDateTime(date.Value);
			ViewBag.DentistID = dentistId;
			//---------------------------------------------------
			//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == clinicId);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
			ViewBag.AmTimeSlots = amTimeSlots;
			ViewBag.PmTimeSlots = pmTimeSlots;

			List<TimeSlot> timeSlots = GenerateTimeSlots(new TimeOnly(7, 0), new TimeOnly(21, 0));
			ViewBag.TimeSlots = timeSlots;
			//--------------------------
			//Lấy các lịch điều trị/ Lịch tái khám - BẢNG FUTUREAPPOINTMENT
			var futureListDb = await _context.PeriodicAppointments.Include(f => f.Dentist).ThenInclude(d => d.Account).Where(f => f.PeriodicAppointmentStatus != "Đã Hủy" && f.Dentist_ID == dentistId && f.DesiredDate == DateOnly.FromDateTime(date.Value)).ToListAsync();
			List<int> slotIDs_FutureAppts = new();
			foreach (var sche in futureListDb)
			{
				slotIDs_FutureAppts.AddRange(GenerateTimeSlotIDs(sche.StartTime, sche.EndTime));
			}
			ViewBag.SlotIDs_FutureAppts = slotIDs_FutureAppts;
			//--------------------------------------------------
			return View(await scheduleSubList.ToListAsync());
		}
		//chỉnh trạng thái từng slot 
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditStatusTo_Nghi(int dentistId, DateOnly date, int timeSlotId)
		{
			var schedule = await _context.Schedules.FirstOrDefaultAsync(m => m.DentistID == dentistId && m.Date == date && m.TimeSlotID == timeSlotId);

			if (schedule != null)
			{
				schedule.ScheduleStatus = "Nghỉ";
				_context.Schedules.Update(schedule);
				await _context.SaveChangesAsync();
			}
			else
			{
				_context.Add(new Schedule
				{
					DentistID = dentistId,
					Date = date,
					TimeSlotID = timeSlotId,
					ScheduleStatus = "Nghỉ"
				});
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Edit", new { dentistId = dentistId, date = date });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditStatusTo_ConTrong(int? dentistId, DateOnly? date, int? timeSlotId)
		{
			var schedule = await _context.Schedules.FirstOrDefaultAsync(m => m.DentistID == dentistId && m.Date == date && m.TimeSlotID == timeSlotId);

			if (schedule != null)
			{
				schedule.ScheduleStatus = "Còn Trống";
				_context.Schedules.Update(schedule);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction("Edit", new { dentistId = dentistId, date = date });
		}

		#endregion


		#region Xóa lịch khám - xóa tất cả trừ Lịch "Đã Đặt" và lịch của bảng LỊCH ĐỊNH KÌ
		// POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int? dentistId, DateTime? date) 
		{
			var scheduleSubList = _context.Schedules.Where(p =>
				p.DentistID == dentistId && p.Date == DateOnly.FromDateTime(date.Value) && p.ScheduleStatus != "Đã Đặt" && p.ScheduleStatus != "Đã Hủy" && p.TimeSlotID != 32);
			if (scheduleSubList.Any())
			{
				_context.Schedules.RemoveRange(scheduleSubList);
				await _context.SaveChangesAsync();
			}
			string referer = Request.Headers["Referer"].ToString();
			if (!string.IsNullOrEmpty(referer))
			{
				return Redirect(referer);
			}
			else
			{
				return RedirectToAction("Index");
			}


		}
		#endregion

		private bool ScheduleExists(int id)
		{
			return _context.Schedules.Any(e => e.ID == id);
		}
	}
}
