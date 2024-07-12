using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.BacklogAPI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Security.Claims;

namespace Dental_Clinic_System.Controllers
{
    public class ChatController : Controller
    {
        private readonly DentalClinicDbContext _context;
        private readonly IBacklogAPI _backlogApi;

        public ChatController(DentalClinicDbContext context, IBacklogAPI backlogAPI)
        {
            _context = context;
            _backlogApi = backlogAPI;
        }

        [Authorize(Roles = "Bệnh Nhân")]
        public async Task<IActionResult> Index(int dentistID)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var patient = _context.Accounts.FirstOrDefault(c => c.Email == emailClaim);

            if(patient == null)
            {
                await _backlogApi.SendErrorToWebhookAsync($"Chat Controller || {MethodBase.GetCurrentMethod().Name} Method", string.Join(". ", "Người dùng không tồn tại trong phiên"), "153898");

                return View("Chat");
            }

            ViewBag.patientID = patient.ID;
            ViewBag.dentistID = dentistID;
            return View("Chat");
        }
    }
}
