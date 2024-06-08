using Microsoft.AspNetCore.Mvc;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
	[Area("dentist")]
	public class DentistController : Controller
	{
		public IActionResult DentistSchedule()
		{
			return View();
		}
	}
}
