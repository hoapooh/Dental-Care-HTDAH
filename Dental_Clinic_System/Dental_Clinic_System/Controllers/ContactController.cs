using Dental_Clinic_System.Areas.Admin.DTO;
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
            var amWorkTimes = await _context.WorkTimes
            .Where(w => w.Session == "Sáng")
            .Select(w => new WorkTimeDto
            {
                ID = w.ID,
                DisplayText = $"{w.Session}: {w.StartTime.ToString("HH:mm")} - {w.EndTime.ToString("HH:mm")}"
            })
            .ToListAsync();

            var pmWorkTimes = await _context.WorkTimes
                .Where(w => w.Session == "Chiều")
                .Select(w => new WorkTimeDto
                {
                    ID = w.ID,
                    DisplayText = $"{w.Session}: {w.StartTime.ToString("HH:mm")} - {w.EndTime.ToString("HH:mm")}"
                })
                .ToListAsync();

            var model = new AddClincVM
            {
                AmWorkTimes = new SelectList(amWorkTimes, "ID", "DisplayText"),
                PmWorkTimes = new SelectList(pmWorkTimes, "ID", "DisplayText")
            };
            return View("Contact", model);
		}
	}
}
