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
            var dentistID = 1;

            // Lấy Schedule của dentist cụ thể
            var schedules = await _context.Schedules
                                    .Include(s => s.Dentist)
                                    .Include(s => s.TimeSlot)
                                    .Where(s => s.DentistID == dentistID)
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
                             appointmentID = sa.appointmentID,
                             scheduleDate = sa.scheduleDate,
                             startTime = sa.startTime,
                             endTime = sa.endTime,
                            patientName = patientRecord?.FullName ?? "No Patient"
                        };

            //foreach (var a in result)
            //{
            //    Console.WriteLine(a);
            //}

            // Map to EventVM
            var events = result.Select(s => new EventVM
            {
                Title = s.appointmentID != 0 ? $"#{s.appointmentID} - {s.patientName}" : "",
                Start = s.scheduleDate != null && s.startTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.startTime.Value:HH:mm:ss}" : null,
                End = s.scheduleDate != null && s.endTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.endTime.Value:HH:mm:ss}" : null,
            }).ToList();

            // Lấy thông tin Nha sĩ
            var dentist = await _context.Dentists
                                        .Include(d => d.Account)
                                        .FirstOrDefaultAsync(d => d.ID == dentistID);

            // Gửi thông tin qua View
            ViewBag.dentistAvatar = dentist.Account.Image;
            ViewBag.dentistName = $"{dentist.Account.LastName} {dentist.Account.FirstName}";
            return View(events);
        }






        [HttpGet]
        public async Task<IActionResult> DentistDescription()
        {
            var dentistID = 1;
            var dentist = await _context.Dentists.Where(d => d.ID == dentistID).Include(d => d.Account).FirstAsync();
            ViewBag.DentistAvatar = dentist?.Account.Image;
            ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
            var message = TempData["Message"] as string;
            ViewBag.message = message;
            return View("DentistDescription", dentist);
        }





        [HttpPost]
        public async Task<IActionResult> SendDentistDescription(string? content)
        {
            int dentistID = 1;
            if (string.IsNullOrEmpty(content))
            {
                TempData["Message"] = "Lỗi! Mô tả không được để trống.";
                return RedirectToAction("DentistDescription");
            }
            var dentist = await _context.Dentists.FindAsync(dentistID);
            dentist.Description = content;
            _context.Dentists.Update(dentist);
            await _context.SaveChangesAsync();

            TempData["Message"] = "success";

            return RedirectToAction("DentistDescription");
        }



        [HttpGet]
        public async Task<IActionResult> PatientAppointments()
        {
            var dentistID = 1;
            var appointment = await _context.Appointments
                                    .Include(a => a.Schedule).ThenInclude(s => s.TimeSlot)
                                    .Include(a => a.PatientRecords)
                                    .Include(a => a.Specialty)
                                    .Where(a => a.Schedule.DentistID == 1)
                                    .ToListAsync();
            var dentist = await _context.Dentists.Where(d => d.ID == dentistID).Include(d => d.Account).FirstAsync();
            ViewBag.DentistAvatar = dentist?.Account.Image;
            ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
            ViewBag.Message = TempData["Message"];
            return View("PatientAppointments", appointment);
        }




        [HttpPost]
        public async Task<IActionResult> ChangePatientAppointment(int appointmentID, string appointmentStatus)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentID);
            appointment.AppointmentStatus = appointmentStatus;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            TempData["message"] = "success";
            return RedirectToAction("PatientAppointments");
        }
    }
}
