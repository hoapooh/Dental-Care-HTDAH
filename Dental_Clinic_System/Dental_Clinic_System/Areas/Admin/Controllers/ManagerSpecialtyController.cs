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

		public ManagerSpecialtyController(DentalClinicDbContext context, HiddenSpecialtyService hiddenSpecialty)
		{
			_context = context;
			_hiddenSpecialty = hiddenSpecialty;
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

		[Route("DeleteSpecialty/{id}")]
		public IActionResult DeleteSpecialty(int id)
		{
			_hiddenSpecialty.HiddenSpecialty(id);
			return RedirectToAction(nameof(ListSpecialty));
		}

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
			if (ModelState.IsValid)
			{
                var specialty = await _context.Specialties.FindAsync(model.Id);

				if (specialty == null)
				{
					return NotFound();
				}

				specialty.Name = model.Name;
				specialty.Image = model.Image;
				specialty.Description = model.Description;

                _context.Specialties.Update(specialty);
				await _context.SaveChangesAsync();

				return RedirectToAction(nameof(ListSpecialty));
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

			return View("EditSpecialty", model);
		}
	}
}
