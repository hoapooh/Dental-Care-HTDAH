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
    public class ManagerSpeciatlyController : Controller
    {
        private readonly DentalClinicDbContext _context;
        private readonly HiddenSpecialtyService _hiddenSpecialty;

        public ManagerSpeciatlyController(DentalClinicDbContext context, HiddenSpecialtyService hiddenSpeciatly)
        {
            _context = context;
            _hiddenSpecialty = hiddenSpeciatly;
        }

        //=======================================QUẢN LÝ CHUYÊN KHOA=======================================

        [Route("ListSpeciatly")]
        public async Task<IActionResult> ListSpeciatly()
        {
            var hiddenSpeciatlies = _hiddenSpecialty.GetHiddenSpeciatly();

            var speciatly = await _context.Specialties
                .Where(s => !hiddenSpeciatlies.Contains(s.ID))
                .ToListAsync();

            var speciatlyList = speciatly.Select(s => new ManagerSpeciatlyVM
            {
                Id = s.ID,
                Name = s.Name,
                Image = s.Image,
                Description = s.Description
            }).ToList();

            return View("ListSpeciatly", speciatlyList);
        }

        [Route("DeleteSpeciatly/{id}")]
        public IActionResult DeleteSpeciatly(int id)
        {
            _hiddenSpecialty.HiddenSpeciatly(id);
            return RedirectToAction(nameof(ListSpeciatly));
        }

        //[HttpGet]
        //[Route("EditSpeciatly/{id}")]
        //public async Task<IActionResult> EditSpeciatly(int id)
        //{
        //    var speciatly = await _context.Specialties.FindAsync(id);
        //    if (speciatly == null)
        //    {
        //        return NotFound();
        //    }

        //    var edit = new ManagerSpeciatlyVM
        //    {
        //        Id = speciatly.ID,
        //        Name = speciatly.Name,
        //        Image = speciatly.Image,
        //        Description = speciatly.Description
        //    };
        //    ViewBag.EditModel = edit;
        //    return View(nameof(ListSpeciatly), edit);
        //}

        [HttpPost]
        [Route("EditSpeciatly")]
        public async Task<IActionResult> EditSpeciatly(ManagerSpeciatlyVM model)
        {
            if (ModelState.IsValid)
            {
                var speciatly = await _context.Specialties.FindAsync(model.Id);
                if (speciatly == null)
                {
                    return NotFound();
                }

                speciatly.Name = model.Name;
                speciatly.Image = model.Image;
                speciatly.Description = model.Description;

                _context.Update(speciatly);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ListSpeciatly));
            }
            
            return View(nameof(ListSpeciatly), new List<ManagerSpeciatlyVM> { model });
        }

        //public async Task<IActionResult> HiddenSpecialty(string name)
        //{
        //    var specialty = await _context.Specialties.SingleOrDefaultAsync(s => s.Name == name);
        //    if (specialty == null)
        //    {
        //        return NotFound();
        //    }
        //    _context.Specialties.Remove(specialty);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(ListSpecialty));

        //}
    }
}
