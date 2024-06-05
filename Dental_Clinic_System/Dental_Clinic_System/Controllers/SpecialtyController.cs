using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace Dental_Clinic_System.Controllers
{
	public class SpecialtyController : Controller
	{
		DentalClinicDbContext _context;
        public SpecialtyController(DentalClinicDbContext context)
        {
			_context = context;
        }

        public IActionResult Index()
		{
			var specialties = _context.Specialties.ToList();
			return View("Specialty",specialties);
		}
	}
}
