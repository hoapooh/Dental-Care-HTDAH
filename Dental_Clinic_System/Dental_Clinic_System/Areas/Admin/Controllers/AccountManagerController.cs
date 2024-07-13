//==============================================TÀI KHOẢN QUẢN LÝ================================================
using AutoMapper;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin")]
	public class AccountManagerController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public AccountManagerController(DentalClinicDbContext context)
		{
			_context = context;
		}

		#region Show List Account với Role Quản lý
		//===================LIST ACCOUNT===================
		//join để kết hợp 2 table là  Accounts và Clinics
		//into accountClinicGroup: tạo 1 nhóm kq từ join
		public async Task<IActionResult> ListAccountManager()
		{
			var accountList = await (from account in _context.Accounts
									 join clinic in _context.Clinics
									 on account.ID equals clinic.ManagerID into accountClinicGroup
									 from clinic in accountClinicGroup.DefaultIfEmpty()

									 where account.Role == "Quản Lý" && account.AccountStatus == "Hoạt Động"
									 select new ManagerAccountVM
									 {
										 Id = account.ID,
										 Username = account.Username,
										 FirstName = account.FirstName,
										 LastName = account.LastName,
										 Email = account.Email,
										 PhoneNumber = account.PhoneNumber,
										 Role = account.Role,
										 ClinicName = clinic != null ? clinic.Name : "N/A"
									 }).ToListAsync();

			return View(accountList);
		}
		#endregion

		#region Tìm kiếm (Search)
		//===================TÌM KIẾM===================
		//[Route("SearchAccount")]
		public async Task<IActionResult> SearchAccount(string keyword)
		{
			//Nếu keyword rỗng, sẽ chuyển hướng đến ListAccount
			if (string.IsNullOrEmpty(keyword))
			{
				return RedirectToAction(nameof(ListAccountManager));
			}

			var accountList = await (from account in _context.Accounts
									 join clinic in _context.Clinics
									 on account.ID equals clinic.ManagerID into accountClinicGroup
									 from clinic in accountClinicGroup.DefaultIfEmpty()

									 where account.Role == "Quản Lý" && account.AccountStatus == "Hoạt Động"
											&& account.Username.Contains(keyword)
									 select new ManagerAccountVM
									 {
										 Id = account.ID,
										 Username = account.Username,
										 FirstName = account.FirstName,
										 LastName = account.LastName,
										 Email = account.Email,
										 PhoneNumber = account.PhoneNumber,
										 Role = account.Role,
										 ClinicName = clinic != null ? clinic.Name : "N/A"
									 }).ToListAsync();

			return View("ListAccountManager", accountList);
		}
		#endregion

		#region Thêm tài khoản (Add Account)
		//===================THÊM TÀI KHOẢN===================
		[HttpPost]
		public async Task<IActionResult> AddAccountManager(string username, string password, string phoneNumber, string email, string? address, string lastname, string firstname)
		{
			//Check thông tin trùng lặp
			var existingAccount = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Email == email || a.PhoneNumber == phoneNumber || a.Username == username);

			if (existingAccount != null)
			{
				bool accountUsername = await _context.Accounts.AnyAsync(a => a.Username == username);
				if (accountUsername)
				{
					ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
				}

				bool accountPhoneNumber = await _context.Accounts.AnyAsync(a => a.PhoneNumber == phoneNumber);
				if (accountPhoneNumber)
				{
					ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại");
				}

				bool accountEmail = await _context.Accounts.AnyAsync(a => a.Email == email);
				if (accountEmail)
				{
					ModelState.AddModelError("Email", "Email đã tồn tại");
				}

				//Lấy lại list account để hiển thị
				var accountList = await (from accounts in _context.Accounts
										 join clinic in _context.Clinics
										 on accounts.ID equals clinic.ManagerID into accountClinicGroup
										 from clinic in accountClinicGroup.DefaultIfEmpty()

										 where accounts.Role == "Quản Lý" && accounts.AccountStatus == "Hoạt Động"
										 select new ManagerAccountVM
										 {
											 Id = accounts.ID,
											 Username = accounts.Username,
											 Email = accounts.Email,
											 PhoneNumber = accounts.PhoneNumber,
											 Address = accounts.Address,
											 Role = accounts.Role,
											 ClinicName = clinic != null ? clinic.Name : "N/A"
										 }).ToListAsync();

				return View("ListAccountManager", accountList);
			}

			//Thêm mới account vào DB
			var account = new Account
			{
				Username = username,
				Password = DataEncryptionExtensions.ToMd5Hash(password),
				LastName = lastname,
				FirstName = firstname,
				PhoneNumber = phoneNumber,
				Email = email,
				Address = address ?? "",
				Role = "Quản Lý",
				AccountStatus = "Hoạt Động"
			};

			_context.Accounts.Add(account);
			await _context.SaveChangesAsync();
            TempData["ToastMessageSuccessTempData"] = "Đăng ký tài khoản quản lý thành công";
            return RedirectToAction(nameof(ListAccountManager));
		}
		#endregion

		#region Chỉnh sửa (Edit Account)
		//===================CHỈNH SỬA TÀI KHOẢN===================
		public async Task<IActionResult> EditAccountManager(int id)
		{
			var account = await _context.Accounts.FindAsync(id);
			if (account == null)
			{
				return NotFound();
			}

			var accountVM = new EditAccountVM
			{
				Id = account.ID,
				FirstName = account.FirstName,
				LastName = account.LastName,
				Username = account.Username,
				Email = account.Email,
				DateOfBirth = account.DateOfBirth,
				PhoneNumber = account.PhoneNumber,
				Gender = account.Gender,
				Province = account.Province,
				District = account.District,
				Ward = account.Ward,
				Address = account.Address,
				Role = account.Role
			};

			return View(accountVM);
		}

		[HttpPost]
		public async Task<IActionResult> EditAccountManager(EditAccountVM model)
		{
			if (ModelState.IsValid)
			{
				var account = await _context.Accounts.FindAsync(model.Id);
				if (account == null)
				{
					return NotFound();
				}

				//Kiểm tra Tên đăng nhập đã tồn tại chưa
				if(await _context.Accounts.AnyAsync(a => a.Username == model.Username && a.ID != model.Id))
				{
					ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
					return View(model);
				}

				//Kiểm tra Sđt đã tồn tại chưa
				if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == model.PhoneNumber && a.ID != model.Id))
				{
					ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại");
					return View(model);
				}
				
				//Kiểm tra Email đã tồn tại chưa
				if(await _context.Accounts.AnyAsync(a => a.Email == model.Email && a.ID != model.Id))
				{
					ModelState.AddModelError("Email", "Email đã tồn tại");
					return View(model);
				}

				if (!string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmPassword))
				{
					if (model.NewPassword != model.ConfirmPassword)
					{
						ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
						return View(model);
					}

					if (model.NewPassword.Length < 3)
					{
						ModelState.AddModelError("NewPassword", "Mật khẩu phải có ít nhất 3 ký tự.");
						return View(model);
					}

					account.Password = DataEncryptionExtensions.ToMd5Hash(model.NewPassword);
				}

				account.ID = model.Id;
				account.FirstName = model.FirstName;
				account.LastName = model.LastName;
				account.Username = model.Username;
				account.Email = model.Email;
				account.DateOfBirth = model.DateOfBirth;
				account.PhoneNumber = model.PhoneNumber;
				account.Gender = model.Gender;
				account.Province = model.Province;
				account.District = model.District;
				account.Ward = model.Ward;
				account.Address = model.Address;
				account.Role = model.Role;

				_context.Update(account);
				await _context.SaveChangesAsync();

                TempData["ToastMessageSuccessTempData"] = "Chỉnh sửa tài khoản quản lý thành công";
                //Chuyển đến ListAccount
                return RedirectToAction(nameof(ListAccountManager));
			}

			TempData["ToastMessageFailTempData"] = "Chỉnh sửa tài khoản quản lý thất bại";
			return View(model);
		}
		#endregion

		#region Tài khoản bị khóa (Delete)
		//===================KHÓA TÀI KHOẢN===================
		//[Route("HiddenAccountStatus")]
		[HttpPost]
		public async Task<IActionResult> HiddenAccountStatus(string username, string status)
		{
			var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);

			if (account != null)
			{
				account.AccountStatus = status;
				await _context.SaveChangesAsync();
			}
            TempData["ToastMessageSuccessTempData"] = "Cấm tài khoản quản lý thành công";
            return RedirectToAction(nameof(ListAccountManager));
		}
		#endregion
	}
}
