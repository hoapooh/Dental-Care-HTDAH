using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin,Mini Admin")]
	public class DashboardController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public DashboardController(DentalClinicDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            TempData.Keep("userID");
            return RedirectToAction("GetAppointmentStatus");
        }


        public async Task<IActionResult> GetAppointmentStatus(int? month)
        {
            var adminAccountID = HttpContext.Session.GetInt32("adminAccountID");
            if (adminAccountID == null)
            {
                return RedirectToAction("Login", "AdminAccount", new { area = "Admin" });
            }

            int currentYear = DateTime.Now.Year;
            int currentMonth = month ?? DateTime.Now.Month;
            DateTime today = DateTime.Today;

			//Đạt khám Thành Công/Thất Bại mỗi tháng cho năm hiện tại và năm trước
			var successfulAppointments = await _context.Appointments
                .Where(a => a.CreatedDate.HasValue &&
                            (a.CreatedDate.Value.Year == currentYear || a.CreatedDate.Value.Year == currentYear - 1) &&
                            a.CreatedDate.Value.Month == currentMonth &&
                            a.AppointmentStatus == "Đã Khám")
                .GroupBy(a => new { a.CreatedDate.Value.Year, a.CreatedDate.Value.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var failedAppointments = await _context.Appointments
                .Where(a => a.CreatedDate.HasValue &&
                            (a.CreatedDate.Value.Year == currentYear || a.CreatedDate.Value.Year == currentYear - 1) &&
                            a.CreatedDate.Value.Month == currentMonth &&
                            a.AppointmentStatus == "Đã Hủy")
                .GroupBy(a => new { a.CreatedDate.Value.Year, a.CreatedDate.Value.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var successfulData = new int[2];
            var failedData = new int[2];

            foreach (var item in successfulAppointments)
            {
                if (item.Year == currentYear)
                    successfulData[0] = item.Count;
                else
                    successfulData[1] = item.Count;
            }

            foreach (var item in failedAppointments)
            {
                if (item.Year == currentYear)
                    failedData[0] = item.Count;
                else
                    failedData[1] = item.Count;
            }

			//Tổng Hợp Tác/Từ Chối Yêu Cầu Của Phòng Khám
			var acceptOrderToday = await _context.Orders
                .Where(o => o.CreatedDate.HasValue && o.CreatedDate.Value.Date == today && o.Status == "Đồng Ý")
                .CountAsync();

            var rejectOrderToday = await _context.Orders
                .Where(o => o.CreatedDate.HasValue && o.CreatedDate.Value.Date == today && o.Status == "Từ Chối")
                .CountAsync();

			//Tổng New được đăng lên trong mỗi Tháng
			var newPostPerMonth = await _context.News
                .Where(n => n.CreatedDate.Year == currentYear)
                .GroupBy(n => n.CreatedDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var newsData = new int[12];
            foreach (var item in newPostPerMonth)
            {
                newsData[item.Month - 1] = item.Count;
            }

            //Xếp hạng Phòng khám theo Rating
            var clinics = await _context.Clinics
                .Where(c => c.Rating.HasValue)
                .Select(c => new { c.Name, c.Rating })
                .ToListAsync();

            var clinicName = clinics.Select(c => c.Name).ToList();
            var clinicRating = clinics.Select(c => c.Rating.Value).ToList();

            //Thêm mới vào DashboardVM
            var model = new DashboardVM
            {
                SelectedYear = currentYear,
                SelectedMonth = currentMonth,
                SuccessfulAppointmentsCurrentYear = successfulData[0],
                SuccessfulAppointmentsPreviousYear = successfulData[1],
                FailedAppointmentsCurrentYear = failedData[0],
                FailedAppointmentsPreviousYear = failedData[1],
                AcceptedOrdersToday = acceptOrderToday,
                RejectedOrdersToday = rejectOrderToday,
                MonthlyNewPost = newsData.ToList(),
                ClinicNames = clinicName,
                ClinicRatings = clinicRating
            };

            return View(model);
        }

    }

}

//SuccessfulAppointments = successfulAppointmentsPerMonth.Sum(x => x.Count),
//FailedAppointments = failedAppointmentsPerMonth.Sum(x => x.Count),