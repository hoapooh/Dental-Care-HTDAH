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
		#region Bảng lịch khám vs các Column: Nha sĩ - Ngày - Các timeslot - Edit
		// GET: Manager/Schedules
		public async Task<IActionResult> Index(int? dentistId, DateTime? date, string status)
		{
			//---------------------------------------------------
			//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == 1);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
			ViewBag.AmTimeSlots = amTimeSlots;
			ViewBag.PmTimeSlots = pmTimeSlots;

			List<TimeSlot> timeSlots = GenerateTimeSlots(new TimeOnly(7, 0), new TimeOnly(21, 0));
			ViewBag.TimeSlots = timeSlots;
			//--------------------------------------------------
			ViewBag.Dentists = await _context.Dentists.Include(d => d.Account).ToListAsync();
			ViewBag.Status = null;

			//--------------------------------------------------
			// Filter schedules from today onwards
			var today = DateOnly.FromDateTime(DateTime.Today);
			var schedulesQuery = _context.Schedules
					.Include(s => s.Dentist)
					.ThenInclude(d => d.Account)
					.Include(s => s.TimeSlot)
					.AsQueryable().Where(s => s.Date >= today);
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

			return View("Index", dentalClinicDbContext);
		}
		public async Task<IActionResult> ViewHistory(int? dentistId, DateTime? date)
		{
            ViewBag.Dentists = await _context.Dentists.Include(d => d.Account).ToListAsync();
			
			var today = DateOnly.FromDateTime(DateTime.Today);
			var schedulesQuery = _context.Schedules
					.Include(s => s.Dentist)
					.ThenInclude(d => d.Account)
					.Include(s => s.TimeSlot)
					.AsQueryable().Where(s => s.Date < today && s.ScheduleStatus == "Đã Đặt");
			if (dentistId.HasValue)
			{
				schedulesQuery = schedulesQuery.Where(s => s.DentistID == dentistId.Value);
			}

			if (date.HasValue)
			{
				schedulesQuery = schedulesQuery.Where(s => s.Date == DateOnly.FromDateTime(date.Value));
			}
			var dentalClinicDbContext = await schedulesQuery.OrderBy(s => s.Date).ToListAsync();

			return View("ViewHistory", dentalClinicDbContext);
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
						  .Where(ts => ts.StartTime >= startTime && ts.EndTime <= endTime).ToList();

		}
		#endregion

		#region Lich làm việc của Nha Sĩ - Hàng tuần - Lịch này hỗ trợ generatr lịch khám theo tuần nhanh chóng
		// GET: Manager/LichLamViec
		public async Task<IActionResult> GetWorkingSchedule()
		{
			//Lịch làm việc sáng chiều của phòng khám. Mô phỏng: clinicID = 1
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == 1);
			ViewBag.SangStart = clinic.AmWorkTimes.StartTime;
			ViewBag.SangEnd = clinic.AmWorkTimes.EndTime;
			ViewBag.ChieuStart = clinic.PmWorkTimes.StartTime;
			ViewBag.ChieuEnd = clinic.PmWorkTimes.EndTime;

			//--------------------------------------------
			// Lấy các lịch làm việc của các nha sĩ thuộc phòng khám cụ thể - mà manager đang quản lý
			//Ví dụ phòng khám có ID=1 (Nha khoa đại dương)
			var den_sesList = _context.Dentist_Sessions.Include(d => d.Dentist).ThenInclude(a => a.Account).Include(d => d.Session).AsQueryable();
			den_sesList = den_sesList.Where(p => p.Dentist.ClinicID == 1); //Lấy tất cả các dòng của Dentist_Sessions where DentistIDs thuộc Clinic có id=1 
			ViewBag.DenSesList = await den_sesList.ToListAsync(); ;
			//return View(await den_sesList.ToListAsync());
			return View();
		}


		// POST: Manager/LichLamViec
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GetWorkingSchedule(List<int> SelectedDenSesList)
		{
			//Lịch làm việc sáng chiều của phòng khám. Mô phỏng: clinicID = 1
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == 1);
			ViewBag.SangStart = clinic.AmWorkTimes.StartTime;
			ViewBag.SangEnd = clinic.AmWorkTimes.EndTime;
			ViewBag.ChieuStart = clinic.PmWorkTimes.StartTime;
			ViewBag.ChieuEnd = clinic.PmWorkTimes.EndTime;

			//--------------------------------------------
			// Lấy các lịch làm việc của các nha sĩ thuộc phòng khám cụ thể - mà manager đang quản lý
			//Ví dụ phòng khám có ID=1 (Nha khoa đại dương)
			var den_sesList = _context.Dentist_Sessions.Include(d => d.Dentist).ThenInclude(a => a.Account).Include(d => d.Session).AsQueryable();
			den_sesList = den_sesList.Where(p => p.Dentist.ClinicID == 1); //Lấy tất cả các dòng của Dentist_Sessions where DentistIDs thuộc Clinic có id=1 
			foreach (var denses in den_sesList)
			{
				if (SelectedDenSesList.Contains(denses.ID)) //nếu được chọn
					denses.Check = true;
				else
					denses.Check = false;
				_context.Update(denses);
			}
			await _context.SaveChangesAsync();
			//Lấy danh sách để in ra
			den_sesList = den_sesList.Where(p => p.Dentist.ClinicID == 1); //Lấy tất cả các dòng của Dentist_Sessions where DentistIDs thuộc Clinic có id=1 
			ViewBag.DenSesList = await den_sesList.ToListAsync();
			return View(await den_sesList.ToListAsync());
		}
		#endregion

		// POST: Manager/LichLamViec
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateWeekSchedule(string selectedDates)
		{
			if (!string.IsNullOrEmpty(selectedDates))
			{
				List<DateOnly> dates = selectedDates.Split(',')
					.Select(date => DateOnly.ParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture))
					.ToList();

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
					.Include(ds => ds.Session)
					.Where(ds => ds.Dentist.ClinicID == 1 && ds.Check == true)
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

			return RedirectToAction("Index");
		}

		private void CreateLichKham(int denId, DateOnly date, List<int> timeSlots)
		{
			foreach (var slot in timeSlots)
			{
				var existSchedule = _context.Schedules.FirstOrDefault(
					a => a.DentistID == denId && a.Date == date && a.TimeSlotID == slot);
				if (existSchedule == null)
				{
					var newSchedule = new Schedule
					{
						DentistID = denId,
						Date = date,
						TimeSlotID = slot,
						ScheduleStatus = "Còn Trống"
					};
					_context.Add(newSchedule);
				}
			}
			_context.SaveChanges();
		}




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

		#region Xóa lịch khám theo ID - autoCreate
		// GET: Manager/Schedules/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var schedule = await _context.Schedules
				.Include(s => s.Dentist)
				.Include(s => s.TimeSlot)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (schedule == null)
			{
				return NotFound();
			}

			return View(schedule);
		}
		#endregion

		#region Tạo lịch khám => Chọn nhiều nha sĩ, chọn nhiều ngày, chọn nhiều timeslot => Nhiều lịch khám
		// GET: Manager/Schedules/Create
		public IActionResult Create()
		{
			var dentists = _context.Dentists
						   .Join(_context.Accounts,
								 dentist => dentist.AccountID,
								 account => account.ID,
								 (dentist, account) => new
								 {
									 DentistID = dentist.ID,
									 FullName = account.LastName + " " + account.FirstName
								 })
						   .ToList();

			ViewData["DentistID"] = new SelectList(dentists, "DentistID", "FullName");
			return View();
		}

		// POST: Manager/Schedules/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("DentistIDs, Dates, TimeSlots")] ScheduleVM schedule)
		{
			if (ModelState.IsValid)
			{
				List<DateOnly> dateList = ConvertStringToDateOnlyList(schedule.Dates);
				foreach (var dentist in schedule.DentistIDs)
				{
					foreach (var date in dateList)
					{
						foreach (var slot in schedule.TimeSlots)
						{
							var existSchedule = await _context.Schedules.FirstOrDefaultAsync(
								a => a.DentistID == dentist && a.Date == date && a.TimeSlotID == slot);
							if (existSchedule == null)
							{
								var newSchedule = new Schedule
								{
									//DentistID = schedule.DentistIDs,
									DentistID = dentist,
									Date = date,
									TimeSlotID = slot,
									ScheduleStatus = "Còn Trống"
								};
								_context.Add(newSchedule);
							}
						}
					}
				}

				//------------
				await _context.SaveChangesAsync();
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

		#region Sửa lịch khám - Vô bảng riêng của "Nha Sĩ - Ngày" cụ thể -> Xóa lịch chưa được đặt 
		// GET: Manager/Schedules/Edit/5
		public async Task<IActionResult> Edit(int? dentistId, DateTime? date)
		{
			if (dentistId == null || date == null)
			{
				return NotFound();
			}

			var scheduleSubList = _context.Schedules.Include(s => s.Dentist).ThenInclude(d => d.Account).Include(s => s.TimeSlot).Where(p =>
				p.DentistID == dentistId && p.Date == DateOnly.FromDateTime(date.Value));


			var dentist = await _context.Dentists.Include(d => d.Account).FirstOrDefaultAsync(m => m.ID == dentistId);
			ViewBag.DentistName = dentist.Account.LastName + " " + dentist.Account.FirstName;
			ViewBag.Date = DateOnly.FromDateTime(date.Value);
			ViewBag.DentistID = dentistId;
			//---------------------------------------------------
			//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
			var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == 1);
			var amID = clinic.AmWorkTimeID;
			var pmID = clinic.PmWorkTimeID;
			List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
			List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
			ViewBag.AmTimeSlots = amTimeSlots;
			ViewBag.PmTimeSlots = pmTimeSlots;
			//--------------------------------------------------
			return View(await scheduleSubList.ToListAsync());

			//-----

		}

		// POST: Manager/Schedules/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ID,DentistID,TimeSlotID,Date,ScheduleStatus")] Schedule schedule)
		{
			if (id != schedule.ID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(schedule);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ScheduleExists(schedule.ID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["DentistID"] = new SelectList(_context.Dentists, "ID", "ID", schedule.DentistID);
			ViewData["TimeSlotID"] = new SelectList(_context.TimeSlots, "ID", "ID", schedule.TimeSlotID);
			return View(schedule);
		}
		#endregion
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
		public async Task<IActionResult> EditStatusTo_ConTrong(int? dentistId, DateOnly? date , int? timeSlotId)
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
		#region Xóa lịch khám theo ID - autoCreate
		// GET: Manager/Schedules/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var schedule = await _context.Schedules
				.Include(s => s.Dentist).ThenInclude(d => d.Account)
				.Include(s => s.TimeSlot)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (schedule == null)
			{
				return NotFound();
			}

			return View(schedule);
		}

		// POST: Manager/Schedules/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var schedule = await _context.Schedules
				.Include(s => s.Dentist).ThenInclude(d => d.Account)
				.Include(s => s.TimeSlot)
				.FirstOrDefaultAsync(m => m.ID == id);
			var denId = schedule.Dentist.ID;
			var date = schedule.Date;
			if (schedule != null)
			{
				_context.Schedules.Remove(schedule);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction("Edit", new { dentistId = denId, date = date });
		}
		#endregion

		private bool ScheduleExists(int id)
		{
			return _context.Schedules.Any(e => e.ID == id);
		}
	}
}
