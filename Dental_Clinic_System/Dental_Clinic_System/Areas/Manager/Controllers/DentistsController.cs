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
using System.Globalization;
using System.Text;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace Dental_Clinic_System.Controllers
{
	[Area("Manager")]
	[Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
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
		public async Task<IActionResult> Index(string keyword, string searchStatus)
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			
			var dentists = await _context.Dentists.Include(d => d.Account).Include(d => d.Clinic).Include(d => d.Degree).Where(d => d.ClinicID == clinicId).ToListAsync();

			if (searchStatus != null)
			{
				ViewBag.SearchStatus = searchStatus;
				dentists = dentists.Where(d => d.Account.AccountStatus == searchStatus).ToList();
			}
			else
			{
				ViewBag.SearchStatus = "Hoạt Động";
				dentists = dentists.Where(d => d.Account.AccountStatus == "Hoạt Động").ToList();
			}	
			if (!string.IsNullOrEmpty(keyword))
			{
				keyword = keyword.Trim().ToLower();
				keyword = Util.ConvertVnString(keyword);
				dentists = dentists.Where(p =>
					(p.Account.FirstName != null && Util.ConvertVnString(p.Account.FirstName).Contains(keyword)) ||
					(p.Account.LastName != null && Util.ConvertVnString(p.Account.LastName).Contains(keyword)) ||
					((p.Account.FirstName != null || p.Account.LastName != null) && Util.ConvertVnString(p.Account.LastName+" "+ p.Account.FirstName).Contains(keyword)	)||
					(p.Account.Email != null && Util.ConvertVnString(p.Account.Email).Contains(keyword)) ||
					(p.Account.PhoneNumber != null && Util.ConvertVnString(p.Account.PhoneNumber).Contains(keyword)))
					.ToList();
			}
			return View(dentists);
		}
		
		// GET: Dentists/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}

			var dentist = await _context.Dentists
				.Include(d => d.Account)
				.Include(d => d.Clinic)
				.Include(d => d.Degree)
				.FirstOrDefaultAsync(m => m.ID == id);
			if (dentist == null || dentist.ClinicID != clinicId)
			{
				return NotFound();
			}
			//--------------------------ĐỊA CHỈ
			ViewBag.Address = dentist.Account.Address;
			if (ViewBag.Address != null)
			{
				ViewBag.Address += ", ";
			}

			ViewBag.Province = null;
			ViewBag.District = null;
			ViewBag.Ward = null;
			if (dentist.Account.Province != null && dentist.Account.Province != 0)
			{
				ViewBag.Province = ", " + await LocalAPIReverseString.GetProvinceNameById((int)(dentist.Account.Province));
				if (dentist.Account.District != null && dentist.Account.District != 0)
				{
					ViewBag.District = ", " + await LocalAPIReverseString.GetDistrictNameById((int)(dentist.Account.Province), (int)(dentist.Account.District));
					if (dentist.Account.Ward != null && dentist.Account.Ward != 0)
						ViewBag.Ward = await LocalAPIReverseString.GetWardNameById((int)(dentist.Account.District), (int)(dentist.Account.Ward));
				}
			}
			//-------------------------CÁC CHUYÊN KHOA
			var specialtyNames = _context.DentistSpecialties.Include(
				s => s.Specialty).AsQueryable()
				.Where(p => p.DentistID == id && p.Check == true)
				.Select(p => p.Specialty.Name) // Extract the names of the specialties
				.ToList();
			ViewBag.SpecialtyNames = specialtyNames;
			return View(dentist);
		}



		// GET: Dentists/Create 
		public IActionResult Create()
		{
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name");
			ViewData["Specialty"] = new SelectList(_context.Specialties, "ID", "Name");
			return View();
		}

		// POST: Dentists/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

		//Thêm tài khoản -> Thêm Nha sĩ
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("SpecialtyIDs, Username, Password, LastName, FirstName, Gender, PhoneNumber, Email, DegreeID, Description")] AddDentistVM dentist)
		{
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentist.DegreeID);
			ViewData["Specialty"] = new SelectList(_context.Specialties, "ID", "Name", dentist.SpecialtyIDs);
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            //Check thông tin trùng lặp
            var existingAccount = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Email == dentist.Email || a.PhoneNumber == dentist.PhoneNumber || a.Username == dentist.Username);

			if (existingAccount != null)
			{
				//Thấy thông tin bị trùng, thông báo lỗi
				ModelState.AddModelError(string.Empty, "Fail: Tên đăng nhập / Email / Số điện thoại - đã tồn tại");

				return View("Create", dentist);
			}

			//Thêm mới account vào DB
			var newAccount = new Account
			{
				Username = dentist.Username,
				Password = DataEncryptionExtensions.ToMd5Hash(dentist.Password),
				LastName = dentist.LastName,
				FirstName = dentist.FirstName,
				Gender = dentist.Gender,
				PhoneNumber = dentist.PhoneNumber,
				Email = dentist.Email,
				Role = "Nha Sĩ",
				AccountStatus = "Hoạt Động",
				Image = dentist.Gender == "Nam"? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FDentist%2Fdentist-default-men.png?alt=media&token=f519d272-3f65-4269-9d0e-657ad36d4c87" : "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FDentist%2Fdentist-default-women.png?alt=media&token=f7412f77-1ed3-4383-8f37-c0cbe9048bc1"
			};

			_context.Accounts.Add(newAccount);
			await _context.SaveChangesAsync();
			//-----------------------------------------------------------
			//Thêm mới nha sĩ
			var account = await _context.Accounts
				.FirstOrDefaultAsync(a => a.Username == dentist.Username);
			// Debugging
			Console.WriteLine($"AccountID: {account.ID}");
			Console.WriteLine($"DegreeID: {dentist.DegreeID}");
			var newDentist = new Dentist
			{
				AccountID = account.ID,
				ClinicID = clinicId ?? 0,
				DegreeID = dentist.DegreeID,
				Description = dentist.Description
			};
			_context.Dentists.Add(newDentist);
			await _context.SaveChangesAsync();
			//-------------------------------------------------------------
			//Thêm vào bảng NhaSi_ChuyenKhoa
			var den = await _context.Dentists.FirstOrDefaultAsync(a => a.AccountID == account.ID); //lấy Nha sĩ ms tạo
			var speIDs = _context.Specialties.OrderBy(a => a.ID).Select(a => a.ID).ToList(); //lấy các ID củae all chuyên khoa
			bool check;
			var newDen_speList = new List<DentistSpecialty>();
			foreach (var speID in speIDs)
			{
				if (dentist.SpecialtyIDs.Contains(speID))
					check = true;
				else
					check = false;
				newDen_speList.Add( new DentistSpecialty
				{
					DentistID = den.ID,
					SpecialtyID = speID,
					Check = check
				});
			}
			_context.AddRange(newDen_speList);
			await _context.SaveChangesAsync();
			//--------------------------------------
			//Thêm dòng cho nha sĩ ms trong Lịch làm việc Dentist_Session
			var newDenSesList = new List<Dentist_Session>();
			for (int i = 1; i <= 14; i++)
			{
				newDenSesList.Add( new Dentist_Session
				{
					Dentist_ID = den.ID,
					Session_ID = i,
					Check = false //mặc định khi tạo ms
				});
			}
			_context.AddRange(newDenSesList);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		// GET: Dentists/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}

			var dentist = await _context.Dentists
								.Include(d => d.Account)
								.FirstOrDefaultAsync(d => d.ID == id);
			if (dentist == null || dentist.Account == null || dentist.ClinicID != clinicId)
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
				Description = dentist.Description ?? "",
				Status = dentist.Account.AccountStatus,
				Image = dentist.Account.Image ?? ""
			};
			// Xử lý các chuyên khoa - BẢNG DENTIST_SPECIALTY
			var den_speList = _context.DentistSpecialties.Include(d => d.Dentist).Include(d => d.Specialty).AsQueryable();
			var speIDs = await den_speList.Where(p => p.DentistID == id && p.Check == true).Select(p => p.SpecialtyID).ToListAsync(); //lấy tất cả ID của chuyên khoa của nha sĩ có DentistID=id
			ViewBag.SpeIDs = speIDs;
			//-------------------
			ViewData["Specialty"] = new SelectList(_context.Specialties, "ID", "Name");
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentist.DegreeID);
			return View(dentistForm);
		}

		// POST: Dentists/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("SpecialtyIDs, DentistId, AccountId, LastName, FirstName, Gender, Province, Ward, District, Address, DateOfBirth, PhoneNumber, Email, DegreeID, Description, Status, Image")] EditDentistVM dentistForm, string? newPass)
		{
			if (id != dentistForm.DentistId)
			{
				return NotFound();
			}

			if (ModelState.IsValid )
			{
				try
				{
					//---------------BẢNG ACCOUNT CỦA NHA SĨ
					var account = _context.Accounts.Find(dentistForm.AccountId);
					if (account != null)
					{
						if(!string.IsNullOrEmpty(newPass))
						{
							account.Password = DataEncryptionExtensions.ToMd5Hash(newPass);
						}
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
						account.AccountStatus = dentistForm.Status;
						account.Image = dentistForm.Image;
					};
					_context.Update(account);
					await _context.SaveChangesAsync();
					//-----------------BẢNG NHA SĨ
					var dentist = _context.Dentists.Find(dentistForm.DentistId);
					if (dentist != null)
					{
						dentist.DegreeID = dentistForm.DegreeID;
						dentist.Description = dentistForm.Description;
					}
					_context.Update(dentist);
					await _context.SaveChangesAsync();
					//------------------BẢNG NHA SĨ_CHUYÊN KHOA
					var den_speList = _context.DentistSpecialties.AsQueryable().Where(d => d.DentistID == id).ToList();
					foreach (var denspe in den_speList)
					{
						if (dentistForm.SpecialtyIDs.Contains(denspe.SpecialtyID))
							denspe.Check = true;
						else
							denspe.Check = false;
						_context.Update(denspe);
					}
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

				TempData["ToastMessageSuccessTempData"] = "Chỉnh sửa thành công!";
				return RedirectToAction(nameof(Details), new { id = id });
			}

			//ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentistForm.DegreeID);
			//ViewData["Specialty"] = new SelectList(_context.Specialties, "ID", "Name");
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
		private bool DentistExists(int id)
		{
			return _context.Dentists.Any(e => e.ID == id);
		}
	}
}
