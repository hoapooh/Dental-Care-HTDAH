using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.BacklogAPI;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var patient = await _context.Accounts.FirstOrDefaultAsync(c => c.Email == emailClaim);
            var dentist = await _context.Dentists.Include(d => d.Account).FirstOrDefaultAsync(d => d.ID == dentistID);
            var dentistSpecialty = await _context.DentistSpecialties.FirstOrDefaultAsync(ds => ds.DentistID == dentistID);
            if(patient == null || dentist == null || dentistSpecialty == null)
            {
                await _backlogApi.SendErrorToWebhookAsync($"Chat Controller || {MethodBase.GetCurrentMethod().Name} Method", string.Join(". ", "Người dùng không tồn tại trong phiên"), "153898");

                return View("Chat");
            }

            var messages = await _context.ChatHubMessages
            .Where(m => (m.SenderId == patient.ID && m.ReceiverId == dentistID) || (m.SenderId == dentistID && m.ReceiverId == patient.ID))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

            var accounts = await _context.Accounts.ToDictionaryAsync(a => a.ID, a => a.Role);

            ViewBag.patientID = patient.ID;
            ViewBag.dentistID = dentistID;
            ViewBag.DentistAvatar = dentist.Account.Image;
            ViewBag.DentistName = (dentist.Account.LastName + " " + dentist.Account.FirstName).Trim();
            ViewBag.SpecialtyID = dentistSpecialty.SpecialtyID;
            ViewBag.Accounts = accounts;
            ViewBag.Messages = messages;

            return View("Chat");
        }
    }
}
