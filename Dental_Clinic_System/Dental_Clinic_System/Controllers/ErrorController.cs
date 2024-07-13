using Microsoft.AspNetCore.Mvc;

namespace Dental_Clinic_System.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode?}")]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                ViewData["StatusCode"] = statusCode.Value;
            }

            return View("NotFound");
        }
    }
}
