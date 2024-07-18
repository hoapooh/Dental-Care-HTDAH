using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
	[Area("Manager")]
	[Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
	public class ReviewsController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public ReviewsController(DentalClinicDbContext context)
		{
			_context = context;
		}

		//[Route("Index")]
		// GET: Dentists
		public async Task<IActionResult> Index(string keyword)
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
			if (clinicId == null)
			{   // Check if session has expired, log out
				return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
			}
			var reviews = await _context.Reviews.Include(r => r.Dentist).ThenInclude(d => d.Account).Include(r => r.Patient).Where(r => r.Dentist.ClinicID == clinicId).ToListAsync();

			if (!string.IsNullOrEmpty(keyword))
			{
				keyword = keyword.Trim().ToLower();
				keyword = Util.ConvertVnString(keyword);
				reviews = reviews.Where(p =>
					(p.Dentist.Account.FirstName != null && Util.ConvertVnString(p.Dentist.Account.FirstName).Contains(keyword)) ||
					(p.Dentist.Account.LastName != null && Util.ConvertVnString(p.Dentist.Account.LastName).Contains(keyword)) ||
					((p.Dentist.Account.FirstName != null || p.Dentist.Account.LastName != null) && Util.ConvertVnString(p.Dentist.Account.LastName + " " + p.Dentist.Account.FirstName).Contains(keyword)) ||
					(p.Rating != null && Util.ConvertVnString(p.Rating.ToString()).Contains(keyword)))
					.ToList();
			}
			return View(reviews);
		}
	}
}
