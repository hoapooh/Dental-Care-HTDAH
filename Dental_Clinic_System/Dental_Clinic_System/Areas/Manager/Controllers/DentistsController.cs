using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using System.Configuration;

namespace Dental_Clinic_System.Controllers
{
	[Area("Manager")]
	//[Route("Manager/[controller]")]
	public class DentistsController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public DentistsController(DentalClinicDbContext context)
		{
			_context = context;
		}
		//[Route("Index")]
		// GET: Dentists
		public async Task<IActionResult> Index()
		{
			var dentists = _context.Dentists.Include(d => d.Account).Include(d => d.Clinic).Include(d => d.Degree);
			return View(await dentists.ToListAsync());
		}
		//[Route("Search")]
		public async Task<IActionResult> Search(String keyword)
		{
			var dentists = _context.Dentists.Include(d => d.Account).Include(d => d.Clinic).Include(d => d.Degree).AsQueryable();
			if (keyword != null)
			{
				keyword = keyword.Trim();
				dentists = dentists.Where(p =>
					(p.Account.FirstName != null && p.Account.FirstName.Contains(keyword)) ||
					(p.Account.LastName != null && p.Account.LastName.Contains(keyword)) ||
					(p.Account.Email != null && p.Account.Email.Contains(keyword)));
			}
			return View("Index", await dentists.ToListAsync());
		}

		// GET: Dentists/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var dentist = await _context.Dentists
				.Include(d => d.Account)
				.Include(d => d.Clinic)
				.Include(d => d.Degree)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (dentist == null)
			{
				return NotFound();
			}

			return View(dentist);
		}
        


        // GET: Dentists/Create 
        public IActionResult Create()
		{
			ViewData["AccountID"] = new SelectList(_context.Accounts, "ID", "AccountStatus");
			ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "Name");
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name");
			return View();
		}

		// POST: Dentists/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		
		/*[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,AccountID,ClinicID,DegreeID,Description")] Dentist dentist)
		{
			if (ModelState.IsValid)
			{
				_context.Add(dentist);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["AccountID"] = new SelectList(_context.Accounts, "ID", "AccountStatus", dentist.AccountID);
			ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "ID", dentist.ClinicID);
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentist.DegreeID);
			return View(dentist);
		}*/

		//Thêm tài khoản -> Thêm Nha sĩ
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(string Username, string Password, string Lastname, string Firstname, string Gender, string PhoneNumber, string Email, string Degree, string Description)
		{
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", Degree);
			//Check thông tin trùng lặp
			var existingAccount = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Email == Email || a.PhoneNumber == PhoneNumber || a.Username == Username);

			if (existingAccount != null)
			{
				//Thấy thông tin bị trùng, thông báo lỗi
				ModelState.AddModelError(string.Empty, "Fail: Tên đăng nhập / Email / Số điện thoại - đã tồn tại");

				return View("Create");
			}

			//Thêm mới account vào DB
			var newAccount = new Account
			{
				Username = Username,
				Password = Password,
				LastName = Lastname,
				FirstName = Firstname,
				Gender = Gender,
				PhoneNumber = PhoneNumber,
				Email = Email,
				Role = "Nha Sĩ",
				AccountStatus = "Hoạt động"
			};

			_context.Accounts.Add(newAccount);
			await _context.SaveChangesAsync();
			//Thêm mới nha sĩ
			var account = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Username == Username);
						// Debugging
						Console.WriteLine($"AccountID: {account.ID}");
						Console.WriteLine($"DegreeID: {Degree}");
			var newDentist = new Dentist
			{
				AccountID = account.ID,
				ClinicID = int.Parse("1"),
				DegreeID = int.Parse(Degree),
				Description = Description
			};
			_context.Dentists.Add(newDentist);
			await _context.SaveChangesAsync();
			//Về xem danh sách
			var dentists = _context.Dentists.Include(d => d.Account).Include(d => d.Clinic).Include(d => d.Degree);
			return View("Index", await dentists.ToListAsync());
		}

		// GET: Dentists/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var dentist = await _context.Dentists.FindAsync(id);
			if (dentist == null)
			{
				return NotFound();
			}
			ViewData["AccountID"] = new SelectList(_context.Accounts, "ID", "AccountStatus", dentist.AccountID);
			ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "ID", dentist.ClinicID);
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "ID", dentist.DegreeID);
			return View(dentist);
		}

		// POST: Dentists/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ID,AccountID,ClinicID,DegreeID,Description")] Dentist dentist)
		{
			if (id != dentist.ID)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(dentist);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DentistExists(dentist.ID))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["AccountID"] = new SelectList(_context.Accounts, "ID", "AccountStatus", dentist.AccountID);
			ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "ID", dentist.ClinicID);
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "ID", dentist.DegreeID);
			return View(dentist);
		}

		// GET: Dentists/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var dentist = await _context.Dentists
				.Include(d => d.Account)
				.Include(d => d.Clinic)
				.Include(d => d.Degree)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (dentist == null)
			{
				return NotFound();
			}

			return View(dentist);
		}

		// POST: Dentists/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var dentist = await _context.Dentists.FindAsync(id);
			if (dentist != null)
			{
				_context.Dentists.Remove(dentist);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool DentistExists(int id)
		{
			return _context.Dentists.Any(e => e.ID == id);
		}
	}
}
