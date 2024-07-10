using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin")]
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


        public async Task<IActionResult> GetAppointmentStatus(int? year)
        {
            var adminAccountID = HttpContext.Session.GetInt32("adminAccountID");
            if (adminAccountID == null)
            {
                return RedirectToAction("Login", "AdminAccount", new { area = "Admin" });
            }

            int currentYear = year ?? DateTime.Now.Year;
            DateTime today = DateTime.Today;

            //Đặt khám thành công/thất bại mỗi tháng
            var successfulAppointmentsPerMonth = await _context.Appointments
                .Where(a => a.CreatedDate.HasValue && a.CreatedDate.Value.Year == currentYear && a.AppointmentStatus == "Đã Khám")
                .GroupBy(a => a.CreatedDate.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var failedAppointmentsPerMonth = await _context.Appointments
                .Where(a => a.CreatedDate.HasValue && a.CreatedDate.Value.Year == currentYear && a.AppointmentStatus == "Đã Hủy")
                .GroupBy(a => a.CreatedDate.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var successfulData = new int[12];
            var failedData = new int[12];

            foreach (var item in successfulAppointmentsPerMonth)
            {
                successfulData[item.Month - 1] = item.Count;
            }

            foreach (var item in failedAppointmentsPerMonth)
            {
                failedData[item.Month - 1] = item.Count;
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
                .Select(c => new { c.Name, c.Rating})
                .ToListAsync();

            var clinicName = clinics.Select(c => c.Name).ToList();
            var clinicRating = clinics.Select(c => c.Rating.Value).ToList();

            //Thêm mới vào DashboardVM
            var model = new DashboardVM
            {
                SelectedYear = currentYear,
                MonthlySuccessfulAppointments = successfulData.ToList(),
                MonthlyFailedAppointments = failedData.ToList(),
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