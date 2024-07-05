using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Controllers
{
	public class ContactController : Controller
	{
        private readonly DentalClinicDbContext _context;

        public ContactController(DentalClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
		{
            #region Using DTO
            //var amStartTimes = await _context.WorkTimes
            //.Where(w => w.Session == "Sáng")
            //.Select(w => new WorkTimeVM
            //{
            //    Time = w.StartTime,
            //    DisplayText = $"{w.StartTime.ToString("HH:mm")}"
            //})
            //.ToListAsync();

            //var amEndTimes = await _context.WorkTimes
            //.Where(w => w.Session == "Sáng")
            //.Select(w => new WorkTimeVM
            //{
            //    Time = w.StartTime,
            //    DisplayText = $"{w.EndTime.ToString("HH:mm")}"
            //})
            //.ToListAsync();

            //var pmStartTimes = await _context.WorkTimes
            //    .Where(w => w.Session == "Chiều")
            //    .Select(w => new WorkTimeVM
            //    {
            //        Time = w.StartTime,
            //        DisplayText = $"{w.StartTime.ToString("HH:mm")}"
            //    })
            //    .ToListAsync();

            //var pmEndTimes = await _context.WorkTimes
            //    .Where(w => w.Session == "Chiều")
            //    .Select(w => new WorkTimeVM
            //    {
            //        Time = w.StartTime,
            //        DisplayText = $"{w.EndTime.ToString("HH:mm")}"
            //    })
            //    .ToListAsync();

            //var model = new AddClincVM
            //{
            //    AmStartTimes = new SelectList(amStartTimes, "Time", "DisplayText"),
            //    AmEndTimes = new SelectList(amEndTimes, "Time", "DisplayText"),

            //    PmStartTimes = new SelectList(pmStartTimes, "Time", "DisplayText"),
            //    PmEndTimes = new SelectList(pmEndTimes, "Time", "DisplayText")
            //};
            #endregion

            ViewBag.AmTimes = new List<TimeOnly>() {  // 7:00, 7:30, 8:00...
                new TimeOnly(7, 0),  new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
                new TimeOnly(9, 0),  new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
                new TimeOnly(11, 0),  new TimeOnly(11, 30), new TimeOnly(12, 0)
            };
            ViewBag.PmTimes = new List<TimeOnly>() {  // 7:00, 7:30, 8:00...
                new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0),  new TimeOnly(13, 30),
                new TimeOnly(14, 0), new TimeOnly(14, 30),
                new TimeOnly(15, 0),  new TimeOnly(15, 30), new TimeOnly(16, 0), new TimeOnly(16, 30),
                new TimeOnly(17, 0),  new TimeOnly(17, 30), new TimeOnly(18, 0), new TimeOnly(18, 30),
                new TimeOnly(19, 0),  new TimeOnly(19, 30), new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
            };

            return View("Contact");
		}
	}
}
