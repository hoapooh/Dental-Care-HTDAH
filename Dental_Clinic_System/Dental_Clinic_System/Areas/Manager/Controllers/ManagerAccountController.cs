using Dental_Clinic_System.Areas.Manager.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var user = _context.Accounts.FirstOrDefault(d => username == d.Username && DataEncryptionExtensions.ToMd5Hash(password) == d.Password);
            if (user == null)
            {
                //ViewBag.ToastFailMessage = "Sai Tên đăng nhập hoặc Mật khẩu";
                TempData["ToastMessageFailTempData"] = "Sai Tên đăng nhập hoặc Mật khẩu.";
                return RedirectToAction("Login");
                //ViewBag.ErrorMessage = "Invalid username or password";
                //return BadRequest("Sai Tên đăng nhập hoặc Mật khẩu");
            }

            if (user.Role == "Quản Lý")
            {
                var clinic = _context.Clinics.Include(c => c.Manager).AsQueryable().FirstOrDefault(s => s.ManagerID == user.ID);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                };

                var claimsIdentity = new ClaimsIdentity(claims, "ManagerScheme");
                var authProperties = new AuthenticationProperties { IsPersistent = true }; //người dùng không cần phải đăng nhập lại sau khi đóng và mở lại trình duyệt của họ

                await HttpContext.SignInAsync("ManagerScheme", new ClaimsPrincipal(claimsIdentity), authProperties);
                HttpContext.Session.SetInt32("managerAccountID", user.ID);
				HttpContext.Session.SetString("name",user.LastName + " " + user.FirstName);
				HttpContext.Session.SetString("image", clinic?.Image ?? "");
				HttpContext.Session.SetInt32("clinicId", clinic?.ID ?? 0);

                //if (!string.IsNullOrEmpty(returnUrl))
                //{
                //    TempData["ToastMessageSuccessTempData"] = "Đăng nhập thành công!";
                //    return Redirect(returnUrl);
                //}
                //else
                //{
                    ViewBag.ToastMessageSuccess = "Đăng nhập thành công!";
                    TempData["ToastMessageSuccessTempData"] = "Đăng nhập thành công.";
                    return RedirectToAction("Profile", "ManagerAccount", new { area = "Manager" });
                //}
            }

            TempData["ToastMessageFailTempData"] = "Tài khoản không hợp lệ (không phải tài khoản của quản lý).";
            return View();

            //ViewBag.ErrorMessage = "Invalid role";
            //return NotFound("Account của bạn có Role không hợp lệ, vui lòng thử lại!");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ManagerScheme");
            return LocalRedirect("/Manager");
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
        public async Task<IActionResult> Profile()
        {
            int? managerAccountID = HttpContext.Session.GetInt32("managerAccountID");
            if (managerAccountID == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }

            // Fetch the model data from the database
            var model = await _context.Accounts
                                  .Where(a => a.ID == managerAccountID && a.Role == "Quản Lý")
                                  .Select(a => new ManagerVM
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
        public async Task<IActionResult> EditProfile(ManagerVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ToastMessageFailTempData"] = "Định dạng nhập không hợp lệ.";
                return RedirectToAction("Profile", "ManagerAccount", new { area = "Manager" });
            }

            int? managerAccountID = HttpContext.Session.GetInt32("managerAccountID");
            if (managerAccountID == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }

            var manager = await _context.Accounts.FirstAsync(a => a.ID == managerAccountID && a.Role == "Quản Lý");

            if (manager == null)
            {
                TempData["ToastMessageFailTempData"] = "Không tìm thây người dùng.";
                return RedirectToAction("Profile", "ManagerAccount", new { area = "Manager" });
            }

            var existedEmail = await _context.Accounts.FirstAsync(a => a.Email == model.Email) == null;
            if (existedEmail)
            {
                TempData["ToastMessageFailTempData"] = "Email đã tồn tại.";
                model.Email = manager.Email;
                return RedirectToAction("Profile", "ManagerAccount", new { area = "Manager" });
            }

            var existedPhoneNumber = await _context.Accounts.FirstAsync(a => a.PhoneNumber == model.PhoneNumber) == null;
            if (existedPhoneNumber)
            {
                TempData["ToastMessageFailTempData"] = "Số điện thoại đã tồn tại.";
                model.PhoneNumber = manager.PhoneNumber;
                return RedirectToAction("Profile", "ManagerAccount", new { area = "Manager" });
            }

            manager.Gender = model.Gender;
            manager.FirstName = model.FirstName;
            manager.LastName = model.LastName;
            manager.Email = model.Email;
            manager.PhoneNumber = model.PhoneNumber;
            manager.DateOfBirth = model.DateOfBirth;
            manager.Province = model.Province;
            manager.District = model.District;
            manager.Ward = model.Ward;
            manager.Address = model.Address;
            manager.Image = model.Image;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("name", manager.LastName + " " + manager.FirstName);

            TempData["ToastMessageSuccessTempData"] = "Lưu thay đổi thành công.";
            return RedirectToAction("Profile", "ManagerAccount", new { area = "Manager" });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            int? managerAccountID = HttpContext.Session.GetInt32("managerAccountID");
            if (managerAccountID == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == managerAccountID && a.Role == "Quản Lý");

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
