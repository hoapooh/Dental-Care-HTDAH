using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
	[Area("Manager")]
	public class ManagerAccountController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public ManagerAccountController(DentalClinicDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public IActionResult Login(string? returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}
		//public IActionResult Login(string returnUrl = "/Manager/Dentists/Index")
		//{
		//	ViewBag.ReturnUrl = returnUrl;
		//	return View();
		//}
		//public IActionResult Login()
		//{
		//	ViewBag.ReturnUrl = "/Manager/ManagerAccount/Login";
		//	return View();
		//}
		[HttpPost]
		public async Task<IActionResult> Login(string username, string password, string? returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			var user = _context.Accounts.FirstOrDefault(d => username == d.Username && password == d.Password);
			if (user == null)
			{
				//ViewBag.ToastFailMessage = "Sai Tên đăng nhập hoặc Mật khẩu";
				TempData["ToastMessageFailTempData"] = "Sai Tên đăng nhập hoặc Mật khẩu";
				return RedirectToAction("Login");	
				//ViewBag.ErrorMessage = "Invalid username or password";
				//return BadRequest("Sai Tên đăng nhập hoặc Mật khẩu");
			}

			if (user.Role == "Quản Lý")
			{
				var clinic = _context.Clinics.Include(c => c.Manager).AsQueryable().FirstOrDefault(s => s.ManagerID == user.ID);
				string clinicID = clinic?.ID.ToString() ?? "";
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.Username),
					new Claim(ClaimTypes.Role, user.Role),
				};

				var claimsIdentity = new ClaimsIdentity(claims, "ManagerScheme");
				var authProperties = new AuthenticationProperties { IsPersistent = true }; //người dùng không cần phải đăng nhập lại sau khi đóng và mở lại trình duyệt của họ

				await HttpContext.SignInAsync("ManagerScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
				HttpContext.Session.SetInt32("managerAccountID", user.ID);
				HttpContext.Session.SetInt32("clinicId", clinic?.ID ?? 0);
				HttpContext.Session.SetInt32("amWtId", clinic?.AmWorkTimeID ?? 0);
				HttpContext.Session.SetInt32("pmWtId", clinic?.PmWorkTimeID ?? 0);

				//return RedirectToAction("Index", "Dentists", new { area = "Manager" });

				if (!string.IsNullOrEmpty(returnUrl))
				{
					TempData["ToastMessageSuccessTempData"] = "Đăng nhập thành công!";
					return Redirect(returnUrl);
				}
				else
				{
					ViewBag.ToastMessageSuccess = "Đăng nhập thành công!";
					TempData["ToastMessageSuccessTempData"] = "Đăng nhập thành công!";
					return RedirectToAction("Index", "Dentists", new { area = "Manager" });
				}
			}

			TempData["ToastMessageFailTempData"] = "Tài khoản không hợp lệ, vui lòng thử lại!";
			return View();

			//ViewBag.ErrorMessage = "Invalid role";
			//return NotFound("Account của bạn có Role không hợp lệ, vui lòng thử lại!");
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync("ManagerScheme");
			return LocalRedirect("/Manager");
		}
	}
}
