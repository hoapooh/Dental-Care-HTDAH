using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using System.Configuration;
using static Dental_Clinic_System.Helper.LocalAPIReverseString;
using Dental_Clinic_System.Helper;

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
            ViewBag.Address = dentist.Account.Address;
            if (ViewBag.Address != null)
            {
                ViewBag.Address += ", ";
            }

            ViewBag.Province = null;
            ViewBag.District = null;
            ViewBag.Ward = null;
            if (dentist.Account.Province != null)
			{
                ViewBag.Province = ", " + await LocalAPIReverseString.GetProvinceNameById((int)(dentist.Account.Province));
                if (dentist.Account.District != null)
				{
                    ViewBag.District = ", " + await LocalAPIReverseString.GetDistrictNameById((int)(dentist.Account.Province), (int)(dentist.Account.District));
                    if (dentist.Account.Ward != null)
                        ViewBag.Ward = await LocalAPIReverseString.GetWardNameById((int)(dentist.Account.District), (int)(dentist.Account.Ward));
                }
            }
				
            return View(dentist);
		}
        


        // GET: Dentists/Create 
        public IActionResult Create()
		{
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
        public async Task<IActionResult> Create([Bind("Username, Password, LastName, FirstName, Gender, PhoneNumber, Email, DegreeID, Description")] AddDentistVM dentist)
        {
            ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentist.DegreeID);
            //Check thông tin trùng lặp
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == dentist.Email || a.PhoneNumber == dentist.PhoneNumber || a.Username == dentist.Username);

            if (existingAccount != null)
            {
                //Thấy thông tin bị trùng, thông báo lỗi
                ModelState.AddModelError(string.Empty, "Fail: Tên đăng nhập / Email / Số điện thoại - đã tồn tại");

                return View("Create",dentist);
            }

            //Thêm mới account vào DB
            var newAccount = new Account
            {
                Username = dentist.Username,
                Password = dentist.Password,
                LastName = dentist.LastName,
                FirstName = dentist.FirstName,
                Gender = dentist.Gender,
                PhoneNumber = dentist.PhoneNumber,
                Email = dentist.Email,
                Role = "Nha Sĩ",
                AccountStatus = "Hoạt động"
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();
            //Thêm mới nha sĩ
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == dentist.Username);
            // Debugging
            Console.WriteLine($"AccountID: {account.ID}");
            Console.WriteLine($"DegreeID: {dentist.DegreeID}");
            var newDentist = new Dentist
            {
                AccountID = account.ID,
                ClinicID = int.Parse("1"),
                DegreeID = dentist.DegreeID,
                Description = dentist.Description
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

            var dentist = await _context.Dentists
								.Include(d => d.Account)
								.FirstOrDefaultAsync(d => d.ID == id);
            if (dentist == null || dentist.Account == null)
			{
				return NotFound();
			}
			var dentistForm = new EditDentistVM
			{
				DentistId = dentist.ID,
                AccountId = dentist.Account.ID,
				LastName = dentist.Account.LastName ?? "",
				FirstName = dentist.Account.FirstName ?? "",
				Gender = dentist.Account.Gender ?? "",
				PhoneNumber = dentist.Account.PhoneNumber ?? "",
				Email = dentist.Account.Email ?? "",
                Province = dentist.Account.Province ?? 0,
                District = dentist.Account.District ?? 0,
                Ward = dentist.Account?.Ward ?? 0,
                Address = dentist.Account?.Address,
                DateOfBirth = dentist.Account?.DateOfBirth,
                Description = dentist.Description ?? ""
			};
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentist.DegreeID);
			return View(dentistForm);
		}

		// POST: Dentists/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DentistId, AccountId, LastName, FirstName, Gender, Province, Ward, District, Address, DateOfBirth, PhoneNumber, Email, DegreeID, Description")] EditDentistVM dentistForm)
        {
            if (id != dentistForm.DentistId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var account = _context.Accounts.Find(dentistForm.AccountId);
                    if (account != null) 
                    {
                        account.LastName = dentistForm.LastName;
                        account.FirstName = dentistForm.FirstName;
                        account.Gender = dentistForm.Gender;
                        account.PhoneNumber = dentistForm.PhoneNumber;
                        account.Email = dentistForm.Email;
                        account.Province = dentistForm.Province;
                        account.District = dentistForm.District;
                        account.Ward = dentistForm.Ward;
                        account.Address = dentistForm.Address;
                        account.DateOfBirth = dentistForm.DateOfBirth;
                    };
                    _context.Update(account);
                    await _context.SaveChangesAsync();

                    var dentist = _context.Dentists.Find(dentistForm.DentistId);
                    if (dentist != null)
                    {
                        dentist.DegreeID = dentistForm.DegreeID;
                        dentist.Description = dentistForm.Description;
                    }
                    _context.Update(dentist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DentistExists(dentistForm.DentistId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = id });
            }

            ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentistForm.DegreeID);
            //List<string> errors = new List<string>();
            //foreach (var value in ModelState.Values)
            //{
            //    foreach (var error in value.Errors)
            //    {
            //        errors.Add(error.ErrorMessage);
            //    }
            //}
            //string errorMessage = string.Join("\n", errors);
            //return BadRequest(errorMessage);

            return View(dentistForm);
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
