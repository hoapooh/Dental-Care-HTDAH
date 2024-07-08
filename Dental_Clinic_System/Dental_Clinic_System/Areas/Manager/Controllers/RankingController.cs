using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
        public async Task<IActionResult> GetAppointmentStats(int clinicID, string startDate, string endDate)
        {

            Console.WriteLine("========================================");
            Console.WriteLine($"Date = {startDate} | {endDate} from Controller");
            Console.WriteLine($"Clinic = {clinicID} from Controller");
            Console.WriteLine("========================================");

            startDate = startDate.Trim();
            endDate = endDate.Trim();

            var startDateOnly = DateOnly.Parse(startDate);
            var endDateOnly = DateOnly.Parse(endDate);

            var formatedStartDate = DateOnly.ParseExact(startDateOnly.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy");
            var formatedEndDate = DateOnly.ParseExact(endDateOnly.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy");
            startDateOnly = DateOnly.Parse(formatedStartDate);
            endDateOnly = DateOnly.Parse(formatedEndDate);

            var rankings = new Rankings(_context); // Assuming you have the context
            var stats = rankings.GetAppointmentStats(clinicID, startDateOnly, endDateOnly);

            return Json(new { successful = stats[0], canceled = stats[1] });
        }

    }
}
