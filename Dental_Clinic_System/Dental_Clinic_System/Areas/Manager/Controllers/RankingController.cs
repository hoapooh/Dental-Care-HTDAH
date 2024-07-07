using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class RankingController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public RankingController(DentalClinicDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GetAppointmentStats(int clinicId, DateOnly startDate, DateOnly endDate)
        {
            var rankings = new Rankings(_context); // Assuming you have the context
            var stats = rankings.GetAppointmentStats(clinicId, startDate, endDate);

            return Json(new { successful = stats[0], canceled = stats[1] });
        }

    }
}
