using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dental_Clinic_System.Controllers
{
    public class ChatController : Controller
    {
        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult Index(int dentistID)
        {
            var patientID = HttpContext.Session.GetInt32("userID");
            ViewBag.patientID = patientID;
            ViewBag.dentistID = dentistID;
            return View("Chat");
        }
    }
}
