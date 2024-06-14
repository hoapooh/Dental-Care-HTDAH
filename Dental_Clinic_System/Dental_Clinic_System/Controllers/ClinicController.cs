using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize(Roles = "Bệnh Nhân")]
		[HttpGet]
        public async Task<IActionResult> PatientRecord(int clinicID, int specialtyID, int dentistID, string selectedDate, string scheduleID)
        {
            var username = User.Identity.Name;
            Console.WriteLine($"{username}");
            var patientRecord = await _context.PatientRecords
                                        .Include(pr => pr.Account)
                                        .Include(pr => pr.Appointments)
                                        .Where(pr => pr.Account.Email == username)
                                        .ToListAsync();
            foreach (var p in patientRecord)
            {
                Console.WriteLine(p.ID);

			}
            ViewBag.scheduleID = 2;
            ViewBag.specialtyID = specialtyID;
            ViewBag.dentistID = dentistID;
            ViewBag.clinicID = clinicID;
            return View(patientRecord);
        }


        [HttpGet]
        public async Task<IActionResult> CreateNewPatientRecord()
        {

            return View();
        }

        //Hàm nhận dữ liệu hồ sơ bệnh nhân (patient record) nếu chưa có hoặc tạo thêm hồ sơ bệnh nhân
        [HttpPost]
        public async Task<IActionResult> AddPatientRecord()
        {

            return View();
        }

        // Choose a patient record for booking (Method GET is here)
        [HttpGet]
        [Authorize(Roles = "Bệnh Nhân")]
        // Route
        public async Task<IActionResult> ConfirmAppointment(int scheduleID, int patientRecordID, int specialtyID, int clinicID, int dentistID)
        {
            var appointment = new Dictionary<string, object>
            {
                { "Schedule", await _context.Schedules.Include(s => s.TimeSlot).FirstOrDefaultAsync(s => s.ID == scheduleID)},
                { "PatientRecord", await _context.PatientRecords.FirstOrDefaultAsync(pr => pr.ID == patientRecordID)},
                { "Specialty", await _context.Specialties.FirstOrDefaultAsync(sp => sp.ID == specialtyID)},
                { "Dentist", await _context.Dentists.Include(d => d.Account). FirstOrDefaultAsync(d => d.ID == dentistID)},
                { "Clinic", await _context.Clinics. FirstOrDefaultAsync(c => c.ID == clinicID) }
            };

            
            //ViewBag.totalDeposit = totalDeposit;
			return View(appointment);
        }
    }

}
