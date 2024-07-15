using Microsoft.AspNetCore.Mvc;

namespace Dental_Clinic_System.Controllers
{
    public class SupportController : Controller
    {
        public IActionResult Index()
        {
            return View("BookingGuide");
        }

        public IActionResult FAQ()
        {
            return View("FAQ");
        }
    }
}
