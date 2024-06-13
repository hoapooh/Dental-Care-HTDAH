//=======================================QUẢN LÝ CHUYÊN KHOA=======================================

using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/[controller]")]
	public class ManagerSpecialtyController : Controller
	{
		private readonly DentalClinicDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly HiddenSpecialtyService _hiddenSpecialty;

		public ManagerSpecialtyController(DentalClinicDbContext context, HiddenSpecialtyService hiddenSpecialty, IWebHostEnvironment iwebhostenvironment)
		{
			_context = context;
			_hiddenSpecialty = hiddenSpecialty;
			_webHostEnvironment = iwebhostenvironment;
		}

		//===================LIST CHUYÊN KHOA===================
		[Route("ListSpecialty")]
		public async Task<IActionResult> ListSpecialty()
		{
			var hiddenSpecialties = _hiddenSpecialty.GetHiddenSpecialty();

			var specialty = await _context.Specialties
				.Where(s => !hiddenSpecialties.Contains(s.ID))
				.ToListAsync();

			var specialtyList = specialty.Select(s => new ManagerSpecialtyVM
			{
				Id = s.ID,
				Name = s.Name,
				Image = s.Image,
				Description = s.Description
			}).ToList();

			return View("ListSpecialty", specialtyList);
		}

		//===================TÌM KIẾM===================
		[Route("SeachSpecialty")]
		public async Task<IActionResult> SeachSpecialty(string search)
		{
			var hiddenSpecialties = _hiddenSpecialty.GetHiddenSpecialty();

			var specialty = await _context.Specialties
				.Where(s => !hiddenSpecialties.Contains(s.ID) && (string.IsNullOrEmpty(search) || s.Name.Contains(search)))
				.ToListAsync();

			var specialtyList = specialty.Select(s => new ManagerSpecialtyVM
			{
				Id = s.ID,
				Name = s.Name,
				Image = s.Image,
				Description = s.Description
			}).ToList();

			return View(nameof(ListSpecialty), specialtyList);
		}

		//===================CHỈNH SỬA===================
		[HttpGet]
		[Route("EditSpecialty/{id}")]
		public async Task<IActionResult> EditSpecialty(int id)
		{
			var specialty = await _context.Specialties.FindAsync(id);
			if (specialty == null)
			{
				return NotFound();
			}

			var viewModel = new ManagerSpecialtyVM
			{
				Id = specialty.ID,
				Name = specialty.Name,
				Image = specialty.Image,
				Description = specialty.Description
			};

			return View(viewModel);
		}

		[HttpPost]
		[Route("EditSpecialty")]
		public async Task<IActionResult> EditSpecialty(ManagerSpecialtyVM model)
		{
			if (!ModelState.IsValid)
			{
				var specialty = await _context.Specialties.FindAsync(model.Id);

				if (specialty == null)
				{
					return NotFound();
				}

				// Kiểm tra tên chuyên khoa có bị trùng không
				bool isDuplicateName = await _context.Specialties.AnyAsync(s => s.Name == model.Name);
				if (isDuplicateName)
				{
					ModelState.AddModelError("Name", "Tên chuyên khoa đã tồn tại.");
					return View("EditSpecialty", model);
				}

				specialty.Name = model.Name;
				specialty.Description = model.Description;

				if (model.ImageFile != null)
				{
					//Check xem thư mục imagespecialty có tồn tại trong wwwroot không
					//Nếu không, tạo thư mục có tên là imagespecialty
					var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "imagespecialty");
					if (!Directory.Exists(imageDirectory))
					{
						Directory.CreateDirectory(imageDirectory);
					}

					//Lưu tệp vào thư mục 
					var filePath = Path.Combine(imageDirectory, model.ImageFile.FileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await model.ImageFile.CopyToAsync(stream);
					}
					specialty.Image = "/imagespecialty/" + model.ImageFile.FileName;
				}

				_context.Specialties.Update(specialty);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(ListSpecialty));
			}

			//KIẾM TRA LỖI
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

			return View("EditSpecialty", model);
		}

		//===================XÓA CHUYÊN KHOA TẠM THỜI===================
		[Route("DeleteSpecialty/{id}")]
		public IActionResult DeleteSpecialty(int id)
		{
			_hiddenSpecialty.HiddenSpecialty(id);
			return RedirectToAction(nameof(ListSpecialty));
		}

		//===================THÊM CHUYÊN KHOA===================
		[HttpPost]
		[Route("AddSpecialty")]
		public async Task<IActionResult> AddSpecialty(string name, string description, IFormFile imageSpecialty)
		{
			//Check thông tin trùng lặp
			var existingName = await _context.Specialties.FirstOrDefaultAsync(s => s.Name == name);

			if (existingName != null)
			{
				//Thấy thông tin bị trùng, thông báo lỗi
				ModelState.AddModelError(string.Empty, "Tên chuyên khoa đã tồn tại");

				//Lấy lại list specialty để hiển thị
				var specialties = await _context.Specialties.ToListAsync();

				var listSpecialty = specialties.Select(s => new ManagerSpecialtyVM
				{
					Id = s.ID,
					Name = s.Name,
					Image = s.Image,
					Description = s.Description
				}).ToList();

				return View(nameof(ListSpecialty), listSpecialty);
			}

			var specialty = new Specialty
			{
				Name = name,
				Description = description
			};

			//Check có file ảnh không
			if (imageSpecialty != null && imageSpecialty.Length > 0)
			{
				//Check xem thư mục imagespecialty có tồn tại trong wwwroot không
				//Nếu không, tạo thư mục có tên là imagespecialty
				var imageDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "imagespecialty");
				if (!Directory.Exists(imageDirectory))
				{
					Directory.CreateDirectory(imageDirectory);
				}

				//Lưu file ảnh vào thư mục imagespecialty
				var filePath = Path.Combine(imageDirectory, imageSpecialty.FileName);
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await imageSpecialty.CopyToAsync(stream);
				}
				specialty.Image = "/imagespecialty/" + imageSpecialty.FileName;
			}

			_context.Specialties.Add(specialty);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(ListSpecialty));
		}
	}
}
