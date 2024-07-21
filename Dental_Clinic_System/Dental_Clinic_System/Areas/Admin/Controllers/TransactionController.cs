using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin,Mini Admin")]
    public class TransactionController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public TransactionController(DentalClinicDbContext context)
        {
            _context = context;
        }

		public async Task<IActionResult> TransactionHistory(string filter = "All", string search = "")
		{
			// Lấy tất cả dữ liệu cần thiết
			var transactionHistory = await _context.Transactions.Include(t => t.Appointment)
				.ThenInclude(t => t.Schedule)
				.ThenInclude(t => t.Dentist)
				.ThenInclude(t => t.Clinic)
				.ToListAsync();

			if (!string.IsNullOrEmpty(search))
			{
				transactionHistory = transactionHistory.Where(t => t.TransactionCode.Contains(search)).ToList();
			}

			if (filter == "ThanhToan")
			{
				transactionHistory = transactionHistory.Where(t => t.Message == "Thanh toán tiền đặt cọc").ToList();
			}
			else if (filter == "HoanTien")
			{
				transactionHistory = transactionHistory.Where(t => t.Message == "Hoàn tiền thành công do nha sĩ hủy lịch hẹn" || t.Message == "Hoàn tiền thành công do phòng khám quá giờ xác nhận đơn khám" || t.Message == "Hoàn tiền đặt cọc").ToList();
			}

			ViewBag.transactionHistory = transactionHistory;

			ViewData["Filter"] = filter;
			ViewData["Search"] = search;
			return View(transactionHistory);
		}
	}
}
