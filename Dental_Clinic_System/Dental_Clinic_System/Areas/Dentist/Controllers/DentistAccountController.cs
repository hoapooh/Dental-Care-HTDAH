using Dental_Clinic_System.Areas.Dentist.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Experimental.ProjectCache;
using Microsoft.EntityFrameworkCore;
using NuGet.ProjectModel;
using NuGet.Protocol;
using System.Security.Claims;
using System.Text.RegularExpressions;

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
                var dentist = await _context.Dentists.FirstAsync(d => d.AccountID == user.ID);
                HttpContext.Session.SetInt32("dentistID", dentist.ID);
                HttpContext.Session.SetString("name", user.LastName + " " + user.FirstName);
				HttpContext.Session.SetString("avatar", user.Image ?? "");
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

        [HttpGet]
        [Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
        public async Task<IActionResult> Profile()
        {
            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
            }

            // Fetch the model data from the database
            var model = await _context.Accounts
                                  .Where(a => a.ID == dentistAccountID && a.Role == "Nha Sĩ")
                                  .Select(a => new DentistAccountVM
                                  {
                                      FirstName = a.FirstName,
                                      LastName = a.LastName,
                                      PhoneNumber = a.PhoneNumber,
                                      Email = a.Email,
                                      Gender = a.Gender,
                                      DateOfBirth = a.DateOfBirth,
                                      Province = a.Province,
                                      District = a.District,
                                      Ward = a.Ward,
                                      Address = a.Address,
                                      Image = a.Image
                                  }).FirstAsync();

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(DentistAccountVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessageFailTempData"] = "Định dạng nhập không hợp lệ.";
                return RedirectToAction("Profile", "DentistAccount", new { area = "Dentist" });
            }

            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
            }

            var dentist = await _context.Accounts.FirstAsync(a => a.ID == dentistAccountID && a.Role == "Nha Sĩ");
            if (dentist == null)
            {
                TempData["ToastMessageFailTempData"] = "Không tìm thây người dùng.";
                return RedirectToAction("Profile", "DentistAccount", new { area = "Dentist" });
            }

            var existedEmail = await _context.Accounts.FirstAsync(a => a.Email == model.Email) == null;
            if (existedEmail)
            {
                TempData["ToastMessageFailTempData"] = "Email đã tồn tại.";
                model.Email = dentist.Email;
                return RedirectToAction("Profile", "DentistAccount", new { area = "Dentist" });
            }

            var existedPhoneNumber = await _context.Accounts.FirstAsync(a => a.PhoneNumber == model.PhoneNumber) == null;
            if (existedPhoneNumber)
            {
                TempData["ToastMessageFailTempData"] = "Số điện thoại đã tồn tại.";
                model.PhoneNumber = dentist.PhoneNumber;
                return RedirectToAction("Profile", "DentistAccount", new { area = "Dentist" });
            }

            dentist.Gender = model.Gender;
            dentist.FirstName = model.FirstName;
            dentist.LastName = model.LastName;
            dentist.Email = model.Email;
            dentist.PhoneNumber = model.PhoneNumber;
            dentist.DateOfBirth = model.DateOfBirth;
            dentist.Province = model.Province;
            dentist.District = model.District;
            dentist.Ward = model.Ward;
            dentist.Address = model.Address;
            dentist.Image = model.Image;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("name", dentist.FirstName + " " + dentist.LastName);
			HttpContext.Session.SetString("avatar", dentist.Image ?? "");

			TempData["ToastMessageSuccessTempData"] = "Lưu thay đổi thành công.";
            return RedirectToAction("Profile", "DentistAccount", new { area = "Dentist" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
            }
            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == dentistAccountID && a.Role == "Nha Sĩ");

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
