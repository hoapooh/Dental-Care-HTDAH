using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
	[Area("dentist")]
	public class DentistController : Controller
	{
        private readonly DentalClinicDbContext _context;
        public DentistController(DentalClinicDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("DentistSchedule");
        }

        [HttpGet]
        //[Authorize(Roles = "Nha Sĩ")]
        public async Task<IActionResult> DentistSchedule()
        {
            var dentistSchedule = await _context.Dentists
                                    .Include(d => d.Account)
                                        .ThenInclude(a => a.PatientRecords)
                                        .ThenInclude(pr => pr.Appointments)
                                    .Include(d => d.Schedules)
                                        .ThenInclude(s => s.TimeSlot)
                                    .Where(w => w.ID == 1)
                                    .FirstOrDefaultAsync();

            var events = dentistSchedule.Schedules.Select(s => new EventVM
            {
                Title = $"{s.Appointments?.ID.ToString() ?? ""}",
                Start = $"{s.Date:yyyy-MM-dd}T{s.TimeSlot.StartTime:HH:mm:ss}",
                End = $"{s.Date:yyyy-MM-dd}T{s.TimeSlot.EndTime:HH:mm:ss}",
            }).ToList();
            ViewBag.dentistAvatar = dentistSchedule.Account.Image;
			ViewBag.dentistName = dentistSchedule.Account.LastName + dentistSchedule.Account.FirstName;

			return View(events);
        }

        [HttpGet]
        public IActionResult DentistDescription()
        {
            var dentist = _context.Dentists.Where(d => d.ID==1).FirstOrDefault();
            return View("DentistDescription", dentist);
        }

        [HttpPost]
        public async Task<IActionResult> SendDentistDescription(string? content)
        {
            int dentistID = 1;
            if (string.IsNullOrEmpty(content)) content = "";
            var dentist = await _context.Dentists.FindAsync(dentistID);
            dentist.Description = content;
            _context.Dentists.Update(dentist);
            await _context.SaveChangesAsync();

            TempData["descriptionStatus"] = "success";

            return RedirectToAction("DentistDescription");
        }

        [HttpGet]
        public async Task<IActionResult> PatientAppointments()
        {
            var appointment = await _context.Appointments
                                    .Include(a => a.Schedule).ThenInclude(s => s.TimeSlot)
                                    .Include(a => a.PatientRecords)
                                    .Include(a => a.Specialty)
                                    .Where(a => a.Schedule.DentistID == 1)
                                    .ToListAsync();

            return View("PatientAppointments", appointment);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePatientAppointment(int appointmentID, string appointmentStatus)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentID);
            appointment.AppointmentStatus = appointmentStatus;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
			return RedirectToAction("PatientAppointments");
        }
    }
}
