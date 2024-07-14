using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class AdminAccountController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public AdminAccountController(DentalClinicDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Login(string returnUrl = "/Admin/Dashboard/Index")
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(string username, string password)
		{
			var user = _context.Accounts.FirstOrDefault(d => username == d.Username && DataEncryptionExtensions.ToMd5Hash(password) == d.Password);
			if (user == null)
			{
				ViewBag.ErrorMessage = "Tài khoản đăng nhập hoặc mật khẩu không hợp lệ. Vui lòng nhập lại";
				return View();
			}

			if (user.Role == "Admin" || user.Role == "Mini Admin")
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.Username),
					new Claim(ClaimTypes.Role, user.Role)
				};

				var claimsIdentity = new ClaimsIdentity(claims, "GetAppointmentStatus");
				var authProperties = new AuthenticationProperties { IsPersistent = true };

				await HttpContext.SignInAsync("GetAppointmentStatus", new ClaimsPrincipal(claimsIdentity), authProperties);
				HttpContext.Session.SetInt32("adminAccountID", user.ID);
				return RedirectToAction("index", "dashboard", new { area = "admin" });
			}

			ViewBag.ErrorMessage = "Tài khoản này không có quyền truy cập trang tiếp theo, vui lòng thử lại!";
			return View();
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync("GetAppointmentStatus");
			return LocalRedirect("/admin");
		}

		[HttpPost]
		[Authorize(Roles = "Admin,Mini Admin")]
		public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
		{
			int? adminAccountID = HttpContext.Session.GetInt32("adminAccountID");

			var user = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == adminAccountID && a.Role == "Admin" || a.Role == "Mini Admin");

			if (user == null)
			{
				TempData["ToastMessageFailTempData"] = "Mật khẩu thay đổi thất bại. Không tìm thấy người dùng";
				return Json(new { success = false, message = "Mật khẩu thay đổi thất bại." });
			}

			if (DataEncryptionExtensions.ToMd5Hash(oldPassword) != user.Password)
			{
				TempData["ToastMessageFailTempData"] = "Mật khẩu hiện tại không đúng.";
				return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
			}

			if (newPassword != confirmPassword)
			{
				TempData["ToastMessageFailTempData"] = "Mật khẩu mới và mật khẩu xác nhận không giống.";
				return Json(new { success = false, message = "Mật khẩu mới và mật khẩu xác nhận không giống." });
			}

			if (newPassword.Length < 3 || newPassword.Length > 30 || !Regex.IsMatch(newPassword, "^[a-zA-Z][a-zA-Z0-9]*$"))
			{
				TempData["ToastMessageFailTempData"] = "Mật khẩu mới không hợp lệ. Mật khẩu phải có độ dài từ 3 đến 30 ký tự, chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.";
				return Json(new { success = false, message = "Mật khẩu mới không hợp lệ." });
			}

			user.Password = DataEncryptionExtensions.ToMd5Hash(newPassword);
			_context.Accounts.Update(user);
			await _context.SaveChangesAsync();

			TempData["ToastMessageSuccessTempData"] = "Mật khẩu thay đổi thành công.";
			return Json(new { success = true, message = "Mật khẩu thay đổi thành công." });
		}
	}
}
