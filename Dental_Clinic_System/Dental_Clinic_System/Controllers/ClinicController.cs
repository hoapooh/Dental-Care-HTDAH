using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace Dental_Clinic_System.Controllers
{
	public class ClinicController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public ClinicController(DentalClinicDbContext context)
        {
			_context = context;
        }

        public IActionResult Index()
		{
			var clinics = _context.Clinics.ToList();
			return View("Clinic", clinics);
		}
	}
}
