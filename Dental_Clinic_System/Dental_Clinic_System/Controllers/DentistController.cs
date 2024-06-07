using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NuGet.Protocol;

namespace Dental_Clinic_System.Controllers
{
    public class DentistController : Controller
    {
        private readonly DentalClinicDbContext _context;
        public DentistController(DentalClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> DentistSchedule()
        {
            var dentistSchedule = await _context.Dentists
                                    .Include(d => d.Account)
                                        .ThenInclude(a => a.PatientRecords)
                                        .ThenInclude(pr => pr.Appointments)
                                    .Include(d => d.Schedules)
                                        .ThenInclude(s => s.TimeSlot)
                                    .Where(w => w.ID == 1)
                                    .ToListAsync();

            var events = dentistSchedule.SelectMany(d => d.Schedules.Select(s => new EventVM
            {
                Title = $"{s.Appointments?.ID.ToString() ?? ""}",
                Start = $"{s.Date:yyyy-MM-dd}T{s.TimeSlot.StartTime:HH:mm:ss}",
                End = $"{s.Date:yyyy-MM-dd}T{s.TimeSlot.EndTime:HH:mm:ss}",
            })).ToList();

            return View(events);
        }

        [HttpGet]
        public IActionResult DentistDescription()
        {
            return View("DentistDescription");
        }

        [HttpPost]
        public IActionResult EditDentistDescription()
        {
            return Ok();
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
    }
}
