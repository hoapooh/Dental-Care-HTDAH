using AutoMapper;
using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/[controller]")]
	//[Route("Admin/{controller = Admin}/{action}")]
	//[Authorize (Roles = "Admin")]
	public class AccountController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public AccountController(DentalClinicDbContext context)
		{
			_context = context;
		}

		//===================LIST ACCOUNT===================
		//Hiển thị List Account với tùy chọn Parameter Role
		[Route("ListAccount")]
		public async Task<IActionResult> ListAccount(string Role)
		{
			ViewBag.Role = Role;

			var accountsQuery = _context.Accounts
			.Where(a => a.AccountStatus == "Hoạt động");

			if (!string.IsNullOrEmpty(Role))
			{
				accountsQuery = accountsQuery.Where(a => a.Role == Role);
			}

			var accounts = await accountsQuery.ToListAsync();

			var accountList = accounts.Select(a => new ManagerAccountVM
			{
				Id = a.ID,
				Username = a.Username,
				Email = a.Email,
				PhoneNumber = a.PhoneNumber,
				Address = a.Address,
				Role = a.Role
				//Status = a.AccountStatus
			}).ToList();


			return View(accountList);
		}

		//===================TÌM KIẾM===================
		[Route("SearchAccount")]
		public async Task<IActionResult> SearchAccount(string keyword, string role)
		{
			@ViewBag.Role = role;

			//Nếu keyword rỗng, sẽ chuyển hướng đến ListAccount
			if (string.IsNullOrEmpty(keyword))
			{
				return RedirectToAction(nameof(ListAccount), new { Role = role });
			}

			var accountQuesry = _context.Accounts
			.Where(a => a.Username.Contains(keyword) && a.AccountStatus == "Hoạt động");

			if (!string.IsNullOrEmpty(keyword))
			{
				accountQuesry = accountQuesry.Where(a => a.Role == role);
			}

			var account = await accountQuesry.Where(a => a.Username.Contains(keyword)).ToListAsync();

			var accountList = account.Select(a => new ManagerAccountVM
			{
				Id = a.ID,
				Username = a.Username,
				Email = a.Email,
				PhoneNumber = a.PhoneNumber,
				Address = a.Address,
				Role = a.Role
				//Status = a.AccountStatus
			}).ToList();

			return View("ListAccount", accountList);
		}

		//===================THÊM TÀI KHOẢN===================
		[Route("AddAccount")]
		[HttpPost]
		public async Task<IActionResult> AddAccount(string username, string password, string phoneNumber, string email, string address, string role)
		{
			//Check thông tin trùng lặp
			var existingAccount = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Email == email || a.PhoneNumber == phoneNumber || a.Username == username);

			if (existingAccount != null)
			{
				//Thấy thông tin bị trùng, thông báo lỗi
				ModelState.AddModelError(string.Empty, "Thông tin người dùng đã tồn tại.");

				//Lấy lại list account để hiển thị
				var accounts = await _context.Accounts
					.Where(a => a.AccountStatus == "Hoạt động")
					.ToListAsync();

				var accountList = accounts.Select(a => new ManagerAccountVM
				{
					Id = a.ID,
					Username = a.Username,
					Email = a.Email,
					PhoneNumber = a.PhoneNumber,
					Address = a.Address,
					Role = a.Role
				}).ToList();

				// Truyền lại Role và list account vào View
				ViewBag.Role = role;
				return View("ListAccount", accountList);
			}

			//Thêm mới account vào DB
			var account = new Account
			{
				Username = username,
				Password = password,
				PhoneNumber = phoneNumber,
				Email = email,
				Address = address,
				Role = role,
				AccountStatus = "Hoạt động"
			};

			_context.Accounts.Add(account);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(ListAccount), new { role });
		}

		//===================CHỈNH SỬA TÀI KHOẢN===================

		[Route("EditAccount/{id}")]
		public async Task<IActionResult> EditAccount(int id, string role)
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
				Password = account.Password,
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

            ViewBag.Role = role;
            return View(accountVM);
		}

		[HttpPost]
		[Route("EditAccount")]
		public async Task<IActionResult> EditAccount(EditAccountVM model, string role)
		{
			if (ModelState.IsValid)
			{
				var account = await _context.Accounts.FindAsync(model.Id);
				if (account == null)
				{
					return NotFound();
				}

				account.ID = model.Id;
				account.FirstName = model.FirstName;
				account.LastName = model.LastName;
				account.Username = model.Username;
				account.Password = model.Password;
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

				//Chuyển đến ListAccount theo role
				return RedirectToAction(nameof(ListAccount), new { role = role });
			}

			//List<string> errors = new List<string>();
			//foreach (var value in ModelState.Values)
			//{
			//	foreach (var error in value.Errors)
			//	{
			//		errors.Add(error.ErrorMessage);
			//	}
			//}
			//string errorMessage = string.Join("\n", errors);
			//return BadRequest(errorMessage);

			ViewBag.Role = role;
            return View(model);
		}

		//===================KHÓA TÀI KHOẢN===================
		[Route("HiddenAccountStatus")]
		[HttpPost]
		public async Task<IActionResult> HiddenAccountStatus(string username, string status, string role)
		{
			var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);

			if (account != null)
			{
				account.AccountStatus = status;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(ListAccount), new { Role = role });
		}



		//================================LIST TÀI KHOẢN BỊ KHÓA================================
		[Route("ListLockedAccount")]
		public async Task<IActionResult> ListLockedAccount()
		{
			var lockedAccount = await _context.Accounts
				.Where(a => a.AccountStatus == "Bị khóa")
				.ToListAsync();

			var lockedAccountList = lockedAccount.Select(a => new ManagerAccountVM
			{
				Id = a.ID,
				Username = a.Username,
				Email = a.Email,
				PhoneNumber = a.PhoneNumber,
				Address = a.Address,
				Role = a.Role,
				Status = a.AccountStatus
			}).ToList();

			return View(lockedAccountList);
		}

		//===================TÌM KIẾM TÀI KHOẢN BỊ KHÓA===================
		[Route("SearchLockedAccount")]
		public async Task<IActionResult> SearchLockedAccount(string keyword)
		{
			//Nếu keyword rỗng, chuyển hướng đến ListLockedAccount
			if (string.IsNullOrEmpty(keyword))
			{
				return RedirectToAction(nameof(ListLockedAccount));
			}

			var accountLocked = await _context.Accounts
				.Where(a => a.Username.Contains(keyword) && a.AccountStatus == "Bị khóa")
				.ToListAsync();

			var accountLockedList = accountLocked.Select(a => new ManagerAccountVM
			{
				Id = a.ID,
				Username = a.Username,
				Email = a.Email,
				PhoneNumber = a.PhoneNumber,
				Address = a.Address,
				Role = a.Role,
				Status = a.AccountStatus
			}).ToList();

			return View(nameof(ListLockedAccount), accountLockedList);
		}

	}
}
