using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/[controller]")]
	public class ManagerClinicController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public ManagerClinicController(DentalClinicDbContext context)
		{
			_context = context;
		}

        #region Show List Clinic
        //===================LIST CLINIC===================
        [Route("ListClinic")]
		public async Task<IActionResult> ListClinic()
		{
			//Lấy dữ liệu từ cơ sở dữ liệu
			var clinics = await (from clinic in _context.Clinics
								 join account in _context.Accounts
								 on clinic.ManagerID equals account.ID
								 
								 where account.Role == "Quản Lý" && clinic.ClinicStatus == "Hoạt Động"
								 select new ManagerClinicVM
								 {
									 ClinicName = clinic.Name,
									 //Address = clinic.Address,
									 Province = clinic.Province,
									 Image = clinic.Image,
									 Id = clinic.ID,
									 ManagerName = account.LastName + " " + account.FirstName
								 }).ToListAsync();

			return View(clinics);
		}
		#endregion

		#region Tìm kiếm phòng khám (Search)
		//===================TÌM KIẾM===================
		[Route("SearchClinic")]
		public async Task<IActionResult> SearchClinic(string search)
		{
			//Nếu search rỗng, sẽ chuyển hướng đến ListClinic
			if (string.IsNullOrEmpty(search))
			{
				return RedirectToAction(nameof(ListClinic));
			}

			var query = from clinic in _context.Clinics
						join account in _context.Accounts
						on clinic.ManagerID equals account.ID
						where account.Role == "Quản Lý" && clinic.ClinicStatus == "Hoạt Động"
						select new ManagerClinicVM
						{
							ClinicName = clinic.Name,
							Address = clinic.Address,
							Image = clinic.Image,
							ManagerName = account.LastName + " " + account.FirstName
						};

			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(c => c.ClinicName.Contains(search));
			}

			var clinics = await query.ToListAsync();

			var clinicList = clinics.Select(c => new ManagerClinicVM
			{
				ClinicName = c.ClinicName,
				Address = c.Address,
				Image = c.Image,
				ManagerName = c.ManagerName
			}).ToList();

			return View(nameof(ListClinic), clinics);
		}
		#endregion

		#region Thêm phòng khám (ADD)
		//===================THÊM PHÒNG KHÁM===================
		[HttpGet]
		[Route("CreateClinic")]
		public async Task<IActionResult> CreateClinic()
		{
			var unassignedManagers = await _context.Accounts
			.Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
			.Select(a => new
			{
				a.ID,
				FullName = a.LastName + " " + a.FirstName
			}).ToListAsync();

			var model = new AddClincVM
			{
				UnassignedManagers = new SelectList(unassignedManagers, "ID", "FullName")
			};

			return View(model);
		}


		[HttpPost]
		[Route("CreateClinic")]
		public async Task<IActionResult> CreateClinic(AddClincVM model)
		{
			if (ModelState.IsValid)
			{
				//Kiểm tra Phòng khám đã tồn tại chưa
				bool clinicNameExists = await _context.Clinics.AnyAsync(c => c.Name == model.Name);
				if (clinicNameExists)
				{
					ModelState.AddModelError("Name", "Tên phòng khám đã tồn tại.");
				}

				//Kiểm tra Hotline đã tồn tại chưa
				bool clinicPhoneExists = await _context.Clinics.AnyAsync(c => c.PhoneNumber == model.PhoneNumber);
				if (clinicPhoneExists)
				{
					ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại.");
				}

				//Kiểm tra Email phòng khám tồn tại chưa
				bool clinicEmailExists = await _context.Clinics.AnyAsync(c => c.Email == model.Email);
				if (clinicEmailExists)
				{
					ModelState.AddModelError("Email", "Email đã tồn tại.");
				}

				//Nếu đã tồn tại, load lại danh sách người quản lý chưa có gắn phòng khám
				if (!ModelState.IsValid)
				{
					var unassignedManager = await _context.Accounts
						.Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
						.Select(a => new
						{
							a.ID,
							FullName = a.LastName + " " + a.FirstName
						})
						.ToListAsync();

					model.UnassignedManagers = new SelectList(unassignedManager, "ID", "FullName");
					return View(model);
				}


				//Kiểm tra Account Quản lý có tồn tại không, và đúng Role Quản lý chưa
				var manager = await _context.Accounts
					.FirstOrDefaultAsync(a => a.ID == model.ManagerID && a.Role == "Quản Lý");

				if (manager == null)
				{
					ModelState.AddModelError(string.Empty, "Manager not found or is not a manager.");
					return View(model);
				}

				var clinic = new Clinic
				{
					Name = model.Name,
					PhoneNumber = model.PhoneNumber,
					Email = model.Email,
					ManagerID = manager.ID,
					Province = model.Province,
					District = model.District,
					Ward = model.Ward,
					Basis = model.Basis,
					Address = model.Address,
					Description = model.Description,
					Image = model.Image,
					ClinicStatus = "Hoạt Động"
				};

				_context.Clinics.Add(clinic);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(ListClinic));
			}

			var unassignedManagers = await _context.Accounts
				.Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
				.Select(a => new
				{
					a.ID,
					FullName = a.LastName + " " + a.FirstName
				}).ToListAsync();

            model.UnassignedManagers = new SelectList(unassignedManagers, "ID", "FullName");

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
			return View(model);
		}
		#endregion

		#region Chỉnh sửa (Edit Clinic)
		//===================CHỈNH SỬA PHÒNG KHÁM===================
		[HttpGet]
		[Route("EditClinic/{id}")]
		public async Task<IActionResult> EditClinic(int id)
		{
			var clinic = await _context.Clinics.FindAsync(id);
			if (clinic == null)
			{
				return NotFound();
			}

			var model = new AddClincVM
			{
				ID = clinic.ID,
				Name = clinic.Name,
				PhoneNumber = clinic.PhoneNumber,
				Email = clinic.Email,
				ManagerID = clinic.ManagerID,
				Province = clinic.Province,
				District = clinic.District,
				Ward = clinic.Ward,
				Basis = clinic.Basis,
				Address = clinic.Address,
				Description = clinic.Description,
				Image = clinic.Image,
				ClinicStatus = "Hoạt Động",
				UnassignedManagers = new SelectList(await _context.Accounts
					.Where(a => a.Role == "Quản Lý" && (!_context.Clinics.Any(c => c.ManagerID == a.ID) || a.ID == clinic.ManagerID))
					.Select(a => new
					{
						a.ID,
						FullName = a.LastName + " " + a.FirstName
					}).ToListAsync(), "ID", "FullName")
			};

			return View(model);
		}

		[HttpPost]
		[Route("EditClinic")]
		public async Task<IActionResult> EditClinic(AddClincVM model)
		{
			if (ModelState.IsValid)
			{
				////Kiểm tra Tên phòng khám đã tồn tại chưa
				//bool existingName = await _context.Clinics.AnyAsync(c => c.Name == model.Name);
				//if(existingName)
				//	ModelState.AddModelError("Name", "Tên phòng khám đã tồn tại.");
				
				////Kiểm tra Email đã tồn tại chưa
				//bool existingEmail = await _context.Clinics.AnyAsync(c => c.Email == model.Email);
				//if (existingEmail)
				//	ModelState.AddModelError("Email", "Email đã tồn tại.");
				
				////Kiểm tra Số điện thoại đã tồn tại chưa
				//bool existingPhone = await _context.Clinics.AnyAsync(c => c.PhoneNumber == model.PhoneNumber);
				//if (existingPhone)
				//	ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại.");

    //            //Nếu đã tồn tại, load lại danh sách người quản lý chưa có gắn phòng khám
				//if(!ModelState.IsValid)
				//{
    //                var unassignedManager = await _context.Accounts
    //                    .Where(a => a.Role == "Quản lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
    //                    .Select(a => new
    //                    {
    //                        a.ID,
    //                        FullName = a.LastName + " " + a.FirstName
    //                    })
    //                    .ToListAsync();

    //                model.UnassignedManagers = new SelectList(unassignedManager, "ID", "FullName");
    //                return View(model);
    //            }

                var clinic = await _context.Clinics.FindAsync(model.ID);
				if (clinic == null)
				{
					return NotFound();
				}

				clinic.Name = model.Name;
				clinic.PhoneNumber = model.PhoneNumber;
				clinic.Email = model.Email;
				clinic.ManagerID = model.ManagerID;
				clinic.Province = model.Province;
				clinic.District = model.District;
				clinic.Ward = model.Ward;
				clinic.Basis = model.Basis;
				clinic.Address = model.Address;
				clinic.Description = model.Description;
				clinic.Image = model.Image;
				clinic.ClinicStatus = "Hoạt Động";

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(ListClinic));
			}

			//Ghi lại List Manager chưa được chỉ định phòng khám nào
			model.UnassignedManagers = new SelectList(await _context.Accounts
				.Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
				.Select(a => new
				{
					a.ID,
					FullName = a.LastName + " " + a.FirstName
				}).ToListAsync(), "ID", "FullName");

			// Log model state errors
			//List<string> errorMessages = new List<string>();
			//foreach (var value in ModelState.Values)
			//{
			//	foreach (var error in value.Errors)
			//	{
			//		errorMessages.Add(error.ErrorMessage);
			//	}
			//}
			//string errorMessage = string.Join("\n", errorMessages);

			//return BadRequest(errorMessage);
			return View(model);
		}
		#endregion

		#region Ẩn phòng khám (Delete)
		//===================XÓA PHÒNG KHÁM===================
		[Route("HiddenClinic")]
        public async Task<IActionResult> HiddenClinic(string name, string status)
        {
            var clinic = await _context.Clinics.SingleOrDefaultAsync(c => c.Name == name);

            if (clinic != null)
            {
                clinic.ClinicStatus = status;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ListClinic));
        }
        #endregion

    }
}