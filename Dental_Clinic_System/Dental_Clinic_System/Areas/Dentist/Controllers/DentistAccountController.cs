using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Experimental.ProjectCache;
using NuGet.ProjectModel;
using NuGet.Protocol;
using System.Security.Claims;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
    [Area("Dentist")]
    public class DentistAccountController : Controller
    {
        private readonly DentalClinicDbContext _context;
        public DentistAccountController(DentalClinicDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/dentist/dentistdetail/index")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = "/dentist/dentistdetail/index")
        {
            var user = _context.Accounts.FirstOrDefault(d => username == d.Username && DataEncryptionExtensions.ToMd5Hash(password) == d.Password);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Sai tên đăng nhập hoặc mặt khẩu. Vui lòng thử lại";
                return View();
            }

            if(user != null && user.AccountStatus == "Bị Khóa")
            {
                TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ quản lý để biết thêm chi tiết!";
                return View();
            }

            if (user.Role == "Nha Sĩ")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "DentistScheme");
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync("DentistScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
                HttpContext.Session.SetInt32("dentistAccountID", user.ID);
                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return RedirectToAction("index","dentistdetail", new { area = "dentist"});
            }

            TempData["ErrorMessage"] = "Tài khoản này không có quyền truy cập trang tiếp theo, vui lòng thử lại!";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("DentistScheme");
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("login");
        }
    }
}
