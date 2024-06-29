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

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
	[Area("Manager")]
	public class SchedulesController : Controller
	{
		private readonly DentalClinicDbContext _context;
		public SchedulesController(DentalClinicDbContext context)
		{
			_context = context;
		}
		#region Bảng lịch khám vs các Column: Nha sĩ - Ngày - Các timeslot - Edit
		// GET: Manager/Schedules
		public async Task<IActionResult> Index()
		{
			var dentalClinicDbContext = _context.Schedules.Include(s => s.Dentist).ThenInclude(d => d.Account).Include(s => s.TimeSlot);
			return View(await dentalClinicDbContext.ToListAsync());
		}
		#endregion

		#region Lich làm việc của Nha Sĩ - Hàng tuần - Lịch này hỗ trợ generatr lịch khám theo tuần nhanh chóng
		// GET: Manager/LichLamViec
		public async Task<IActionResult> LichLamViec()
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
		public async Task<IActionResult> LichLamViec(List<int> SelectedDenSesList)
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
					{1, (0, 23)}, {2, (0, 24)}, {3, (1, 23)}, {4, (1, 24)},
					{5, (2, 23)}, {6, (2, 24)}, {7, (3, 23)}, {8, (3, 24)},
					{9, (4, 23)}, {10, (4, 24)}, {11, (5, 23)}, {12, (5, 24)},
					{13, (6, 23)}, {14, (6, 24)}
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
									ScheduleStatus = "Còn Trống"
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


		public async Task<IActionResult> CreateWeekSchedule_oldVer(string selectedDates)
		{

			if (!string.IsNullOrEmpty(selectedDates))
			{
				// Generate list dates dựa trên tuần đã chọn
				List<DateOnly> dates = selectedDates
					.Split(',')
					.Select(date => DateOnly.ParseExact(date.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture))
					.ToList(); //1 List<DateOnly> gồm 7 ngày, dates[0] là thứ 2, dates[1] là thứ 3

				//---------------------------------------------------
				//Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
				var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == 1);
				var amID = clinic.AmWorkTimeID;
				var pmID = clinic.PmWorkTimeID;
				List<int> amTimeSlots = GenerateTimeSlots(amID);
				List<int> pmTimeSlots = GenerateTimeSlots(pmID);
				//--------------------------------------------
				// Lấy lịch làm việc của phòng khám gồm ID | DentistID | SessionID (Id, thứ?, sáng/chiều?)
				var den_sesList = _context.Dentist_Sessions.Include(d => d.Dentist).ThenInclude(a => a.Account).Include(d => d.Session).AsQueryable().Where(p => p.Dentist.ClinicID == 1);
				//Lấy all dentist của Lịch làm việ
				List<int> denIdList = den_sesList.Select(ds => ds.Dentist_ID).Distinct().ToList();
				if (denIdList.Count > 0)
				{
					foreach (var denId in denIdList)
					{
						//Lấy ra session của denId -> Cho biết denId này làm vào những thứ mấy? làm sáng ko? làm chiều ko?
						List<int> sesIDs = den_sesList.Where(ds => ds.Dentist_ID == denId && ds.Check == true).Select(ds => ds.Session_ID).ToList();
						//Tạo lịch theo denId, timeslotId, date
						if (sesIDs.Contains(1))
							CreateLichKham(denId, dates[0], amTimeSlots);
						if (sesIDs.Contains(2))
							CreateLichKham(denId, dates[0], pmTimeSlots);
						if (sesIDs.Contains(3))
							CreateLichKham(denId, dates[1], amTimeSlots);
						if (sesIDs.Contains(4))
							CreateLichKham(denId, dates[1], pmTimeSlots);
						if (sesIDs.Contains(5))
							CreateLichKham(denId, dates[2], amTimeSlots);
						if (sesIDs.Contains(6))
							CreateLichKham(denId, dates[2], pmTimeSlots);
						if (sesIDs.Contains(7))
							CreateLichKham(denId, dates[3], amTimeSlots);
						if (sesIDs.Contains(8))
							CreateLichKham(denId, dates[3], pmTimeSlots);
						if (sesIDs.Contains(9))
							CreateLichKham(denId, dates[4], amTimeSlots);
						if (sesIDs.Contains(10))
							CreateLichKham(denId, dates[4], pmTimeSlots);
						if (sesIDs.Contains(11))
							CreateLichKham(denId, dates[5], amTimeSlots);
						if (sesIDs.Contains(12))
							CreateLichKham(denId, dates[5], pmTimeSlots);
						if (sesIDs.Contains(13))
							CreateLichKham(denId, dates[6], amTimeSlots);
						if (sesIDs.Contains(14))
							CreateLichKham(denId, dates[6], pmTimeSlots);
					}
				}
			}



			return View();
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

		private List<int> GenerateTimeSlots(int workTimeId)
		{

			// Retrieve the WorkTime based on the given ID
			var workTime = _context.WorkTimes
								  .FirstOrDefault(wt => wt.ID == workTimeId);

			if (workTime == null)
			{
				Console.WriteLine("WorkTime not found.");
				return new List<int>();
			}

			var startTime = workTime.StartTime;
			var endTime = workTime.EndTime;

			// Retrieve the matching TimeSlots
			return _context.TimeSlots
						  .Where(ts => ts.StartTime >= startTime && ts.EndTime <= endTime)
						  .Select(ts => ts.ID)
						  .ToList();

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

			//ViewData["DentistID"] = new SelectList(_context.Dentists, "ID", "ID");
			//ViewData["TimeSlotID"] = new SelectList(_context.TimeSlots, "ID", "ID");
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

			var scheduleSubList = _context.Schedules.Include(s => s.Dentist).ThenInclude(d => d.Account).Include(s => s.TimeSlot).AsQueryable();
			scheduleSubList = scheduleSubList.Where(p =>
				p.DentistID == dentistId && p.Date == DateOnly.FromDateTime(date.Value));

			var dentist = await _context.Dentists.Include(d => d.Account).FirstOrDefaultAsync(m => m.ID == dentistId);
			ViewBag.DentistName = dentist.Account.LastName + " " + dentist.Account.FirstName;
			ViewBag.Date = DateOnly.FromDateTime(date.Value);

			return View(await scheduleSubList.ToListAsync());

			//-----
			//var schedule = await _context.Schedules.FindAsync(dentistId);
			//         if (schedule == null)
			//         {
			//             return NotFound();
			//         }
			//         ViewData["DentistID"] = new SelectList(_context.Dentists, "ID", "ID", schedule.DentistID);
			//         ViewData["TimeSlotID"] = new SelectList(_context.TimeSlots, "ID", "ID", schedule.TimeSlotID);
			//         return View(schedule);
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
