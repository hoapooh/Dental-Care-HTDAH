using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
            var clinics = await _context.Clinics.ToListAsync();
            return View("clinic", clinics);
        }

		[HttpGet]
        public async Task<IActionResult> ClinicDetail(int clinicID)
        {
			var clinic = _context.Clinics.FirstOrDefault(c => c.ID == clinicID);
			return View(clinic);
        }

		[HttpGet]
        public async Task<IActionResult> ChooseClinicSpecialty(int clinicID)
        {
            var clinic = await _context.Clinics.
                Include(c => c.Dentists).
                    ThenInclude(a => a.Account).
                Include(f => f.Dentists).
                    ThenInclude(d => d.DentistSpecialties).
                    ThenInclude(ds => ds.Specialty).
                FirstOrDefaultAsync(c => c.ID == clinicID);

            if (clinic == null)
            {
                return NotFound();
            }

            var dentistAvailable = clinic.Dentists.Where(d => d.Account.AccountStatus == "Hoạt Động");

            // Lọc ra các Specialty duy nhất
            var specialties = dentistAvailable.SelectMany(d => d.DentistSpecialties).Select(ds => ds.Specialty).Distinct().ToList();

            ViewBag.ClinicID = clinicID; // Lưu clinicID vào ViewBag để sử dụng trong view

            return View("ClinicSpecialty", specialties);
        }

        [HttpGet]
        public async Task<IActionResult> ChooseDentist(int clinicID, int specialtyID, string keysearch = null, int? degreeID = null, string gender = null)
        {
            var dentists = await _context.Dentists
                            .Include(d => d.Account)
                            .Include(d => d.Clinic)
                            .Include(d => d.DentistSpecialties)
                                .ThenInclude(ds => ds.Specialty)
                            .Include(d => d.Degree)
                            .Where(d => d.ClinicID == clinicID && d.Account.AccountStatus == "Hoạt Động" && d.DentistSpecialties.Any(ds => ds.SpecialtyID == specialtyID))
                            .ToListAsync();

            if (degreeID != null)
            {
                dentists = dentists.Where(d => d.Degree.ID == degreeID).ToList();

            }

            if (!string.IsNullOrEmpty(gender))
            {
                dentists = dentists.Where(d => d.Account.Gender == gender).ToList();
            }

            if (!string.IsNullOrEmpty(keysearch))
            {
                dentists = dentists.Where(d => d.Account.LastName.ToLower().Contains(keysearch.ToLower()) || d.Account.FirstName.ToLower().Contains(keysearch.ToLower())).ToList();
            }

            ViewBag.Specialty = dentists.FirstOrDefault()?.DentistSpecialties.FirstOrDefault(ds => ds.SpecialtyID == specialtyID)?.Specialty;
            ViewBag.clinicID = clinicID;
            ViewBag.specialtyID = specialtyID;
            ViewBag.degree = degreeID;
            ViewBag.gender = gender;
            ViewBag.keysearch = keysearch;
            return View("ClinicDentist", dentists);
        }

        //Hàm nhận tất cả schedule của dentistID truyền vào từ URL 
        [HttpGet]
        public async Task<IActionResult> ClinicDentistCalendar(int clinicID, int specialtyID, int dentistID)
        {
            var schedule = await _context.Schedules
                                .Include(s => s.TimeSlot)
                                .Include(s => s.Appointments)
                                .Include(s => s.Dentist)
                                    .ThenInclude(d => d.DentistSpecialties)
                                    .ThenInclude(ds => ds.Specialty)
								.Include(s => s.Dentist)
                                    .ThenInclude(d => d.Account)
								.Where(s => s.DentistID == dentistID)
                                .ToListAsync();
			ViewBag.clinicID = clinicID;
			ViewBag.specialtyID = specialtyID;
            ViewBag.dentistID = dentistID;
			return View(schedule);
        }

        //Hàm trả về JSON tất cả schedule của dentist hiện tại có status là "Còn Trống"
		public IActionResult GetSchedules(int dentistID)
		{
            var schedules = _context.Schedules
                .Include(s => s.TimeSlot)
                .Include(s => s.Dentist)
                .Where(s => s.Dentist.ID == dentistID && s.ScheduleStatus == "Còn Trống")
                .Select(s => new
                {
                    Date = s.Date.ToString("yyyy-MM-dd"),
                    StartTime = s.TimeSlot.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.TimeSlot.EndTime.ToString(@"hh\:mm"),
                    scheduleID = s.ID
                })
				.ToList();
            foreach(var s in schedules)
            {
                Console.WriteLine(s);
            }
			return Json(schedules);
		}

    }

}
