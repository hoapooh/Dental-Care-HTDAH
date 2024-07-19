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
            var successfulPerMonth = new int[12];
            var canceledPerMonth = new int[12];

            if (startDateOnly.Month == 1 && endDateOnly.Month == 12)
            {
                for (int month = 1; month <= 12; month++)
                {
                    var monthStartDate = new DateOnly(startDateOnly.Year, month, 1);
                    var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
                    var monthStats = rankings.GetAppointmentStats(clinicID, monthStartDate, monthEndDate);

                    successfulPerMonth[month - 1] = monthStats[0];
                    canceledPerMonth[month - 1] = monthStats[1];
                }
            }

            var previousStartDate = startDateOnly.AddDays(-(endDateOnly.Day - startDateOnly.Day + 1)); // Adjust for same period in previous month
            var previousEndDate = startDateOnly.AddDays(-1); // End date of previous period

            var previousStats = rankings.GetAppointmentStats(clinicID, previousStartDate, previousEndDate);
            var successfulChange = CalculatePercentageChange(previousStats[0], stats[0]);
            var canceledChange = CalculatePercentageChange(previousStats[1], stats[1]);

            return Json(new
            {
                successful = stats[0],
                canceled = stats[1],
                successfulChange,
                canceledChange,
                successfulPerMonth,
                canceledPerMonth
            });
        }

        private double CalculatePercentageChange(int previousValue, int currentValue)
        {
            if (previousValue == 0)
            {
                return currentValue > 0 ? 100.0 : 0.0;
            }
            return ((double)(currentValue - previousValue) / previousValue) * 100;
        }
    }
}
