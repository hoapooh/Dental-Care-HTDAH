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
		private readonly HiddenSpecialtyService _hiddenSpecialty;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ManagerSpecialtyController(DentalClinicDbContext context, HiddenSpecialtyService hiddenSpecialty, IWebHostEnvironment iwebhostenvironment)
		{
			_context = context;
			_hiddenSpecialty = hiddenSpecialty;
			_webHostEnvironment = iwebhostenvironment;	
		}

		//=======================================QUẢN LÝ CHUYÊN KHOA=======================================

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

        //===================XÓA TẠM THỜI===================
        [Route("DeleteSpecialty/{id}")]
		public IActionResult DeleteSpecialty(int id)
		{
			_hiddenSpecialty.HiddenSpecialty(id);
			return RedirectToAction(nameof(ListSpecialty));
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
			List<string> errors = new List<string>();
			foreach (var value in ModelState.Values)
			{
				foreach (var error in value.Errors)
				{
					errors.Add(error.ErrorMessage);
				}
			}
			string errorMessage = string.Join("\n", errors);
			return BadRequest(errorMessage);

			return View("EditSpecialty", model);
		}

        //===================TÌM KIẾM===================
  //      public async Task<IActionResult> SeachSpecialty(string keyword)
		//{
		//	if (string.IsNullOrWhiteSpace(keyword))
		//	{
		//		return RedirectToAction(nameof(ListSpecialty));
		//	}

		//	var specialty = await _context.Specialties
		//		.Where(s => s.Name.Contains(keyword))
		//		.ToListAsync();

		//	var specialtyList = specialty.Select(s => new ManagerSpecialtyVM
		//	{
		//		Id = s.ID,
		//		Name = s.Name,
		//		Image = s.Image,
		//		Description = s.Description
		//	}).ToList();

		//	return View(nameof(ListSpecialty), specialtyList);
		//}


	}
}
