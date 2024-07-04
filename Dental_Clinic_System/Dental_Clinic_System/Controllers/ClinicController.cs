using AutoMapper.Internal;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Dental_Clinic_System.Controllers
{
    public class ClinicController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public ClinicController(DentalClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clinics = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).ToListAsync();

			// Format the current date and time
			var now = Util.GetUtcPlus7Time();
			var weekdays = new[] { "Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };
			var dayOfWeek = weekdays[(int)now.DayOfWeek];
			var formattedDate = $"{dayOfWeek}, {now:dd/MM/yyyy}";

			// Pass the formatted date to the view
			ViewBag.CurrentDateTime = formattedDate;

			return View("clinic", clinics);
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDetail(int clinicID)
        {
            var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(c => c.ID == clinicID);
            if (clinic == null)
            {
                return NotFound();
            }

            // Get Review from Patient
            var reviews = await _context.Reviews.Include(r => r.Patient).Include(r => r.Dentist).Where(d => d.Dentist.ClinicID == clinicID).ToListAsync();
            ViewBag.Reviews = reviews;

            return View(clinic);
        }

        [HttpGet]
        public async Task<IActionResult> ChooseClinicSpecialty(int clinicID)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Dentists)
                    .ThenInclude(d => d.Account)
                .Include(c => c.Dentists)
                    .ThenInclude(d => d.DentistSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .FirstOrDefaultAsync(c => c.ID == clinicID);

            if (clinic == null)
            {
                return NotFound();
            }

            var dentistAvailable = clinic.Dentists
                .Where(d => d.Account.AccountStatus == "Hoạt Động")
                .ToList();

            var specialties = dentistAvailable
                .SelectMany(d => d.DentistSpecialties)
                .Select(ds => ds.Specialty)
                .Distinct()
                .ToList();

            ViewBag.ClinicID = clinicID;

            return View("ClinicSpecialty", specialties);
        }

        [HttpGet]
        public async Task<IActionResult> ChooseDentist(int clinicID, int specialtyID, string keysearch = null, int? degreeID = null, string gender = null)
        {
            var dentistsQuery = _context.Dentists
                .Include(d => d.Account)
                .Include(d => d.Clinic)
                .Include(d => d.DentistSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Include(d => d.Degree)
                .Where(d => d.ClinicID == clinicID && d.Account.AccountStatus == "Hoạt Động" && d.DentistSpecialties.Any(ds => ds.SpecialtyID == specialtyID));

            if (degreeID != null)
            {
                dentistsQuery = dentistsQuery.Where(d => d.Degree.ID == degreeID);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                dentistsQuery = dentistsQuery.Where(d => d.Account.Gender == gender);
            }

            if (!string.IsNullOrEmpty(keysearch))
            {
                dentistsQuery = dentistsQuery.Where(d => d.Account.LastName.ToLower().Contains(keysearch.ToLower()) || d.Account.FirstName.ToLower().Contains(keysearch.ToLower()));
            }

            var dentists = await dentistsQuery.ToListAsync();

            var firstDentistSpecialty = dentists.FirstOrDefault()?.DentistSpecialties.FirstOrDefault(ds => ds.SpecialtyID == specialtyID)?.Specialty;

            ViewBag.Specialty = firstDentistSpecialty;
            ViewBag.clinicID = clinicID;
            ViewBag.specialtyID = specialtyID;
            ViewBag.degree = degreeID;
            ViewBag.gender = gender;
            ViewBag.keysearch = keysearch;

            return View("ClinicDentist", dentists);
        }

        [HttpGet]
        public async Task<IActionResult> ClinicDentistCalendar(int clinicID, int specialtyID, int dentistID)
        {
            var schedule = await _context.Schedules
                .Include(s => s.TimeSlot)
                .Include(s => s.Appointments)
                .Include(s => s.Dentist)
                    .ThenInclude(d => d.DentistSpecialties)
                    .ThenInclude(ds => ds.Specialty)
                .Where(s => s.DentistID == dentistID)
                .ToListAsync();

            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.ID == clinicID);
            var specialty = await _context.Specialties.FirstOrDefaultAsync(s => s.ID == specialtyID);
            var dentist = await _context.Dentists.Include(d => d.Account).FirstOrDefaultAsync(d => d.ID == dentistID);

            if (clinic == null || specialty == null || dentist == null)
            {
                return NotFound();
            }

            ViewBag.clinicName = clinic.Name;
            ViewBag.clinicAddress = $"{clinic.Address}, {clinic.WardName}, {clinic.DistrictName}, {clinic.ProvinceName}";
            ViewBag.dentistName = $"{dentist.Account.LastName} {dentist.Account.FirstName}";
            ViewBag.specialtyName = specialty.Name;
            ViewBag.clinicID = clinicID;
            ViewBag.specialtyID = specialtyID;
            ViewBag.dentistID = dentistID;

            return View(schedule);
        }

        [HttpGet]
		//     public async Task<IActionResult> GetSchedules(int? dentistID)
		//     {
		//         if (dentistID == null || dentistID == 0)
		//         {
		//             return NotFound("Không tìm thấy nha sĩ này");
		//         }

		//         //Lấy thông tin phòng khám từ nha sĩ
		//         var dentist = _context.Dentists.Include(d => d.Clinic).First(d => d.ID == dentistID);

		//         //Generate 2 list timeSlot dựa trên WorkTime Sáng vs Chiều
		//         var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).FirstOrDefaultAsync(m => m.ID == dentist.Clinic.ID);
		//         var amID = clinic.AmWorkTimeID;
		//         var pmID = clinic.PmWorkTimeID;
		//         List<TimeSlot> amTimeSlots = GenerateTimeSlots(amID);
		//         List<TimeSlot> pmTimeSlots = GenerateTimeSlots(pmID);
		//         foreach (var s in amTimeSlots)
		//         {
		//             Console.WriteLine(s.StartTime + "         " + s.EndTime);
		//         }

		//         foreach (var s in pmTimeSlots)
		//         {
		//             Console.WriteLine(s.StartTime + "         " + s.EndTime);
		//         }

		//         //Generate lịch làm việc ảo dựa theo dentist ID. 

		//         //var today = DateOnly.FromDateTime(DateTime.Today);
		//         DateTime utc7Now = Util.GetUtcPlus7Time().AddHours(12); //Thêm thời gian 12 tiếng cho phù hợp với business rule
		//         DateOnly todayDate = DateOnly.FromDateTime(utc7Now);
		//         TimeOnly todayTime = TimeOnly.FromDateTime(utc7Now);
		//         var schedules = _context.Schedules
		//                 .Include(s => s.Dentist)
		//                 //.ThenInclude(d => d.Account)
		//                 .Include(s => s.TimeSlot)
		//                 .Where(s => (s.Date >= todayDate && s.TimeSlot.StartTime >= todayTime) && s.DentistID == dentistID); // && s.ScheduleStatus == "Còn Trống"
		//																														   //.Select(s => new
		//																														   //{
		//																														   //    s.DentistID,
		//																														   //    Date = s.Date.ToString("yyyy-MM-dd"),
		//																														   //    StartTime = s.TimeSlot.StartTime.ToString("HH:mm"),
		//																														   //    EndTime = s.TimeSlot.EndTime.ToString("HH:mm"),
		//																														   //    scheduleID = s.ID
		//																														   //})
		//																														   //.ToList();

		////Làm sao để lấy được danh sách các future appointment
		////
		////dính tới ngày làm việc của dentist này với số lượng tối đa là 2 slot 

		//var groupData = schedules.GroupBy(s => new { s.Date, s.DentistID });
		//         groupData.Select(gd => new
		//{
		//	Date = gd.Key.Date,
		//	DentistID = gd.Key.DentistID,
		//	TimeSlots = gd.Select(s => new
		//	{
		//		StartTime = s.TimeSlot.StartTime,
		//		EndTime = s.TimeSlot.EndTime,
		//		ScheduleID = s.ID
		//	})
		//});
		//         foreach (var group in groupData)
		//         {
		//             Console.WriteLine(group.Key.Date);
		//         }

		//         foreach (var s in schedules)
		//         {
		//             Console.WriteLine(s);
		//         }

		//         return Json(groupData);
		//     }
		public async Task<IActionResult> GetSchedules(int? dentistID)
		{
			// Lấy thông tin phòng khám từ nha sĩ
			var dentist = await _context.Dentists.Include(d => d.Clinic).FirstOrDefaultAsync(d => d.ID == dentistID);

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
				.Where(s => (s.Date > todayDate || (s.Date >= todayDate && s.TimeSlot.StartTime >= todayTime)) && s.DentistID == dentistID) // && s.ScheduleStatus == "Còn Trống"
				.ToListAsync();

			// Lấy tất cả các ngày có trong schedules
			var allDates = schedules.Select(s => s.Date).Distinct().ToList();

			// Tạo danh sách các time slot với thời gian 30 phút cho từng ngày
			var timeSlots = new List<object>();
			foreach (var date in allDates)
			{
				var dailyTimeSlots = new List<object>();

				foreach (var slot in amTimeSlots.Concat(pmTimeSlots))
				{
					var startTime = slot.StartTime;
					var endTime = slot.EndTime;
					while (startTime < endTime)
					{
						var nextTime = startTime.AddMinutes(30);
						if (nextTime > endTime) nextTime = endTime;

						dailyTimeSlots.Add(new
						{
							Date = date.ToString("yyyy-MM-dd"),
							StartTime = startTime.ToString("HH:mm"),
							EndTime = nextTime.ToString("HH:mm"),
							ScheduleID = (int?)null
						});

						startTime = nextTime;
					}
				}

				// Add dailyTimeSlots vào timeSlots chung
				timeSlots.AddRange(dailyTimeSlots);
			}

			return Json(timeSlots);
		}





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
