using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/[controller]")]
	public class DashboardController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public DashboardController(DentalClinicDbContext context)
		{
			_context = context;
		}

        [HttpGet("appointment-status")]
        public IActionResult GetAppointmentStatus()
        {
            // Mặc định hiển thị dữ liệu cho ngày hôm nay
            DateTime today = DateTime.Today;
            return View(today);
        }

        [HttpPost("appointment-status")]
        public async Task<IActionResult> GetAppointmentStatus(DateTime date)
        {
            // Đặt ngày giới hạn từ 00:00 đến 23:59 của ngày cần thống kê
            DateTime startDate = date.Date;
            DateTime endDate = date.Date.AddDays(1).AddSeconds(-1);

            // Truy vấn list cuộc hẹn trong ngày
            var appointments = await _context.Appointments
                .Where(a => a.CreatedDate >= startDate && a.CreatedDate <= endDate)
                .ToListAsync();

            // Đếm số lượng người đặt thành công và thất bại
            int successfulAppointment = appointments.Count(a => a.AppointmentStatus == "Thành công");
            int failedAppointment = appointments.Count(a => a.AppointmentStatus == "Thất bại");

            // Trả về dữ liệu thống kê
            var status = new
            {
                SuccessfulAppointments = successfulAppointment,
                FailedAppointments = failedAppointment
            };

            return Ok(status);
        }
    }
}
