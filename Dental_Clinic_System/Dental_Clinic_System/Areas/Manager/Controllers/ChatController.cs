using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
	[Area("Manager")]
	[Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
	public class ChatController : Controller
	{

		private readonly DentalClinicDbContext _context;

		public ChatController(DentalClinicDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int patientID, string searchStatus)
		{
			var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }

            var dentists = await _context.Dentists.Include(d => d.Account).Include(d => d.Clinic).Include(d => d.Degree).Where(d => d.ClinicID == clinicId).ToListAsync();

            if (searchStatus != null)
            {
                ViewBag.SearchStatus = searchStatus;
                dentists = dentists.Where(d => d.Account.AccountStatus == searchStatus).ToList();
            }
            else
            {
                ViewBag.SearchStatus = "Hoạt Động";
                dentists = dentists.Where(d => d.Account.AccountStatus == "Hoạt Động").ToList();
            }

            return View(dentists);
		}

        public async Task<IActionResult> ChatList(int dentistID)
        {
            var chats = await _context.ChatHubMessages
                .Where(m => m.SenderId == dentistID || m.ReceiverId == dentistID)
                .Select(m => new
                {
                    Id = m.ID,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderName = m.Sender.FirstName  + " " + m.Sender.LastName ,
                    ReceiverName = m.Receiver.FirstName + " " + m.Receiver.LastName 
                })
                .GroupBy(m => m.SenderId == dentistID ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                .ToListAsync();

            var accounts = await _context.Accounts.ToDictionaryAsync(a => a.ID, a => a.Role);

            ViewBag.Accounts = accounts;
            ViewBag.ChatList = chats;
            ViewBag.DentistID = dentistID;
            return View();
        }

        public async Task<IActionResult> ChatDentistDetail(int patientID, int dentistID)
        {
            var messages = await _context.ChatHubMessages
                .Where(m => (m.SenderId == dentistID && m.ReceiverId == patientID) || (m.SenderId == patientID && m.ReceiverId == dentistID))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            ViewBag.PatientID = patientID;
            ViewBag.Messages = messages;

            var patient = await _context.Accounts.FirstAsync(a => a.ID == patientID);

            var patientName = $"{patient.FirstName} {patient.LastName}";

            var accounts = await _context.Accounts.ToDictionaryAsync(a => a.ID, a => a.Role);

            ViewBag.Accounts = accounts;
            ViewBag.PatientName = patientName ?? "Ẩn Danh";
            ViewBag.DentistID = dentistID;

            return View("ChatDetail");
        }
    }
}
