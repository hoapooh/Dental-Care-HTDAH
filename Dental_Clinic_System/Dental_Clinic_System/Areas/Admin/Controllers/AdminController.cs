using AutoMapper;
using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/[controller]")]
	//[Route("Admin/{controller = Admin}/{action}")]
	//[Authorize (Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public AdminController(DentalClinicDbContext context)
		{
			_context = context;
		}

		//=======================================QUẢN LÝ TÀI KHOẢN=======================================
		//[Route("Admin/{controller = Admin}/{action = ListAccount}")]
		[Route("ListAccount")]
		public async Task<IActionResult> ListAccount(string Role)
		{
			@ViewBag.Role = Role;

			var accountsQuery = _context.Accounts
			.Where(a => a.Role != "Admin" && a.AccountStatus == "Hoạt động");

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
				Role = a.Role,
				//Gender = a.Gender,
				Status = a.AccountStatus
			}).ToList();
			return View(accountList);
		}

		//[Route("Admin/{controller=Admin}/SearchAccount")]
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
			.Where(a => a.Username.Contains(keyword) && a.AccountStatus == "Hoạt động" && a.Role != "Admin");

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
				Role = a.Role,
				Status = a.AccountStatus
			}).ToList();

			return View("ListAccount", accountList);
		}

		[Route("AddAccount")]
		[HttpPost]
		public async Task<IActionResult> AddAccount(string Username, string Password, string PhoneNumber, string Email, string Address, string Role)
		{
			var newAccount = new Account
			{
				Username = Username,
				Password = Password,
				PhoneNumber = PhoneNumber,
				Email = Email,
				Address = Address,
				Role = Role,
				AccountStatus = "Hoạt động"
			};

			_context.Accounts.Add(newAccount);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(ListAccount), new { Role });
		}

		//[Route("Admin/{controller = Admin}/{action=HiddenAccountStatus}")]
		[Route("HiddenAccountStatus")]
		[HttpPost]
		public async Task<IActionResult> HiddenAccountStatus(string username, string status)
		{
			var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);

			if (account != null)
			{
				account.AccountStatus = status;
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(ListAccount));
		}
	}
}
