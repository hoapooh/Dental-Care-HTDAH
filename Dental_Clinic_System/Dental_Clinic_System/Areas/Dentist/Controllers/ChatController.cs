using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
    [Area("dentist")]
    [Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
    public class ChatController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public ChatController(DentalClinicDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> ChatDetail(int patientID)
        {
            var dentistID = HttpContext.Session.GetInt32("dentistID");

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

            return View();
        }

        public async Task<IActionResult> ChatList()
        {
            var dentistID = HttpContext.Session.GetInt32("dentistID");

            var chats = await _context.ChatHubMessages
                .Where(m => m.SenderId == dentistID || m.ReceiverId == dentistID)
                .Select(m => new
                {
                    Id = m.ID,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    SenderName = m.Sender.FirstName ?? "Ẩn" + " " + m.Sender.LastName ?? "Danh",
                    ReceiverName = m.Receiver.FirstName ?? "Ẩn" + " " + m.Receiver.LastName ?? "Danh"
                })
                .GroupBy(m => m.SenderId == dentistID ? m.ReceiverId : m.SenderId)
                .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                .ToListAsync();

            var accounts = await _context.Accounts.ToDictionaryAsync(a => a.ID, a => a.Role);

            ViewBag.Accounts = accounts;
            ViewBag.ChatList = chats;
            return View();
        }
    }
}
