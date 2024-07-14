using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Google.Apis.PeopleService.v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin")]
	public class AccountSecondAdminController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public AccountSecondAdminController(DentalClinicDbContext context)
		{
			_context = context;
		}

        #region Show List Account Admin phụ
        //===================LIST ACCOUNT===================
        public async Task<IActionResult> ListAccountSecondAdmin()
		{
			var accounts = await _context.Accounts
			.Where(a => a.AccountStatus == "Hoạt Động" && a.Role == "Mini Admin")
			.ToListAsync();

			var accountList = accounts.Select(a => new ManagerAccountVM
			{
				Id = a.ID,
				Username = a.Username,
				FirstName = a.FirstName,
				LastName = a.LastName,
				Email = a.Email,
				PhoneNumber = a.PhoneNumber,
				Gender = a.Gender
			}).ToList();

			return View(accountList);
		}

		#endregion

		#region Tìm kiếm (Search)
		//===================TÌM KIẾM===================
		public async Task<IActionResult> SearchAccount(string keyword)
		{
			//Nếu keyword rỗng, sẽ chuyển hướng đến ListAccountSecondAdmin
			if (string.IsNullOrEmpty(keyword))
			{
				return RedirectToAction(nameof(ListAccountSecondAdmin));
			}

			var account = await _context.Accounts
			.Where(a => a.Username.Contains(keyword) && a.AccountStatus == "Hoạt Động" && a.Role == "Mini Admin")
			.ToListAsync();

			var accountList = account.Select(a => new ManagerAccountVM
			{
				Id = a.ID,
				Username = a.Username,
				FirstName = a.FirstName,
				LastName = a.LastName,
				Email = a.Email,
				PhoneNumber = a.PhoneNumber,
				Gender = a.Gender
			}).ToList();

			return View("ListAccountSecondAdmin", accountList);
		}
		#endregion

		#region Thêm tài khoản (Add Account Admin phụ)
		//===================THÊM TÀI KHOẢN===================
		[HttpPost]
		public async Task<IActionResult> AddAccountSecondAdmin(string username, string password, string firstname, string lastname, string phoneNumber, string email, string gender)
		{
			//Check thông tin trùng lặp
			var existingAccount = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Email == email || a.PhoneNumber == phoneNumber || a.Username == username);

			if (existingAccount != null)
			{
				//Kiểm tra Tên đăng nhập đã tồn tại chưa
				bool accountUsername = await _context.Accounts.AnyAsync(a => a.Username == username);
				if (accountUsername)
					ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");

				//Kiểm tra Email đã tồn tại chưa
				bool accountEmail = await _context.Accounts.AnyAsync(a => a.Email == email);
				if (accountEmail)
					ModelState.AddModelError("Email", "Email đã tồn tại");

				//Kiểm tra Sđt đã tồn tại chưa
				bool accountPhoneNumber = await _context.Accounts.AnyAsync(a => a.PhoneNumber == phoneNumber);
				if (accountPhoneNumber)
					ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại");

				//Lấy lại list account để hiển thị
				var accounts = await _context.Accounts
					.Where(a => a.AccountStatus == "Hoạt Động" && a.Role == "Mini Admin")
					.ToListAsync();

				var accountList = accounts.Select(a => new ManagerAccountVM
				{
					Id = a.ID,
					Username = a.Username,
					FirstName = a.FirstName,
					LastName = a.LastName,
					Email = a.Email,
					PhoneNumber = a.PhoneNumber,
					Gender = a.Gender
				}).ToList();

				return View("ListAccountSecondAdmin", accountList);
			}

			//Thêm mới account vào DB
			var account = new Account
			{
				FirstName = firstname,
				LastName = lastname,
				Username = username,
				Password = DataEncryptionExtensions.ToMd5Hash(password),
				PhoneNumber = phoneNumber,
				Email = email,
				Gender = gender,
				Role = "Mini Admin",
				AccountStatus = "Hoạt Động"
			};

			_context.Accounts.Add(account);
			await _context.SaveChangesAsync();

			TempData["ToastMessageSuccessTempData"] = "Đăng ký tài khoản Admin phụ thành công";
			return RedirectToAction(nameof(ListAccountSecondAdmin));
		}
		#endregion

		#region Chỉnh sửa (Edit Account)
		//===================CHỈNH SỬA TÀI KHOẢN===================
		public async Task<IActionResult> EditAccountSecondAdmin(int id)
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
		public async Task<IActionResult> EditAccountSecondAdmin(EditAccountVM model)
		{
			if (ModelState.IsValid)
			{
				var account = await _context.Accounts.FindAsync(model.Id);
				if (account == null)
				{
					return NotFound();
				}

				//Kiểm tra Tên đăng nhập đã tồn tại chưa
				if (await _context.Accounts.AnyAsync(a => a.Username == model.Username && a.ID != model.Id))
				{
					ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
					return View(model);
				}

				//Kiểm tra Email đã tồn tại chưa
				if (await _context.Accounts.AnyAsync(a => a.Email == model.Email && a.ID != model.Id))
				{
					ModelState.AddModelError("Email", "Email đã tồn tại");
					return View(model);
				}

				//Kiểm tra Sđt đã tồn tại chưa
				if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == model.PhoneNumber && a.ID != model.Id))
				{
					ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại");
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

				TempData["ToastMessageSuccessTempData"] = "Chỉnh sửa tài khoản Admin phụ thành công";
				//Chuyển đến ListAccount
				return RedirectToAction(nameof(ListAccountSecondAdmin));
			}

			TempData["ToastMessageFailTempData"] = "Chỉnh sửa tài khoản Admin phụ thất bại";
			return View(model);
		}
		#endregion

		#region Tài khoản bị khóa (Delete)
		//===================KHÓA TÀI KHOẢN===================
		[HttpPost]
		public async Task<IActionResult> HiddenAccountSecondAdmin(string username, string status)
		{
			var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);

			if (account != null)
			{
				account.AccountStatus = status;
				await _context.SaveChangesAsync();
			}
			TempData["ToastMessageSuccessTempData"] = "Cấm tài khoản Admin phụ thành công";
			return RedirectToAction(nameof(ListAccountSecondAdmin));
		}
		#endregion
	}
}
