using Azure;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.MOMO;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
    [Area("dentist")]
    [Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
    
    public class DentistDetailController : Controller
    {
        private readonly DentalClinicDbContext _context;
        //private readonly IMOMOPayment _momoPayment;
        public DentistDetailController(DentalClinicDbContext context, IMOMOPayment momoPayment)
        {
            _context = context;
            //_momoPayment = momoPayment;
        }

        public IActionResult Index()
        {
            TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
            return RedirectToAction("DentistSchedule");
        }



        #region Lấy lịch làm việc của Dentist, đưa vào Calendar
        [HttpGet]
        //[Authorize(AuthenticationSchemes = "DentistScheme", Roles = "Nha Sĩ")]
        public async Task<IActionResult> DentistSchedule()
        {

            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
            }

            // Lấy Schedule của dentist cụ thể
            var schedules = await _context.Schedules
                                    .Include(s => s.Dentist)
                                    .Include(s => s.TimeSlot)
                                    .Where(s => s.Dentist.Account.ID == dentistAccountID)
                                    .ToListAsync();

            // Map 2 bên Schedule với appointment, sau đó map những cột appointment ko null với patient record
            var scheduleAppointments = from schedule in schedules
                                       join appointment in _context.Appointments
                                       on schedule.ID equals appointment.ScheduleID into appointmentGroup
                                       from appointment in appointmentGroup.DefaultIfEmpty()
                                       select new
                                       {
                                           appointmentID = appointment?.ID ?? 0,
                                           scheduleDate = schedule.Date,
                                           patientID = appointment?.PatientRecordID ?? 0,
                                           startTime = schedule?.TimeSlot?.StartTime,
                                           endTime = schedule?.TimeSlot?.EndTime,
                                           patientRecordID = appointment?.PatientRecordID ?? 0
                                       };


            var result = from sa in scheduleAppointments
                         join patientRecord in _context.PatientRecords
                         on sa.patientRecordID equals patientRecord.ID into patientGroup
                         from patientRecord in patientGroup.DefaultIfEmpty()
                         select new
                         {
                             sa.appointmentID,
                             sa.scheduleDate,
                             sa.startTime,
                             sa.endTime,
                             patientName = patientRecord?.FullName ?? "No Patient"
                         };

            // Map to EventVM
            var events = result.Select(s => new EventVM
            {
                Title = s.appointmentID != 0 ? $"#{s.appointmentID} - {s.patientName}" : "Trống",
                Start = s.scheduleDate != null && s.startTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.startTime.Value:HH:mm:ss}" : null,
                End = s.scheduleDate != null && s.endTime.HasValue ? $"{s.scheduleDate:yyyy-MM-dd}T{s.endTime.Value:HH:mm:ss}" : null,
            }).ToList();


            // Lấy thông tin Nha sĩ
            var dentist = await _context.Dentists
                                        .Include(d => d.Account)
                                        .FirstOrDefaultAsync(d => d.Account.ID == dentistAccountID);

            // Gửi thông tin qua View
            ViewBag.dentistAvatar = dentist?.Account.Image ?? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FPatient%2Fuser.png?alt=media&token=9010a4a6-0220-4d29-bb85-1fe425100744";
            ViewBag.dentistName = $"{dentist?.Account.LastName} {dentist?.Account.FirstName}";
            ViewBag.events = JsonConvert.SerializeObject(events);
            TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
            return View();
        }

        #endregion

        #region Chỉnh sửa thông tin của Dentist

        [HttpGet]
        public async Task<IActionResult> DentistDescription()
        {
            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
            }
            var dentist = await _context.Dentists.Where(d => d.Account.ID == dentistAccountID).Include(d => d.Account).FirstAsync();
            ViewBag.DentistAvatar = dentist?.Account.Image ?? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FPatient%2Fuser.png?alt=media&token=9010a4a6-0220-4d29-bb85-1fe425100744";
            ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
            //Lấy ra những thông báo cần hiển thị
            TempData["ErrorMessage"] = TempData["ErrorMessage"] as string;
            TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
            return View(dentist);
        }





        [HttpPost]
        public async Task<IActionResult> SendDentistDescription(string? content)
        {
            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistAccount", new { area = "dentist" });
            }
            if (string.IsNullOrEmpty(content))
            {
                TempData["ErrorMessage"] = "Lỗi! Mô tả không được để trống.";
                return View();
            }
            var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.AccountID == dentistAccountID);
            dentist.Description = content;
            _context.Dentists.Update(dentist);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thay đổi thành công!";

            return View();
        }

        #endregion

        #region Lấy dữ liệu quản lý lịch đặt khám của bệnh nhân cho nha sĩ

        [HttpGet]
        public async Task<IActionResult> PatientAppointments()
        {
            var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
            if (dentistAccountID == null)
            {
                return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
            }

            var dentist = await _context.Dentists.Where(d => d.Account.ID == dentistAccountID).Include(d => d.Account).FirstAsync();
            var appointments = await _context.Appointments
                                    .Include(a => a.Schedule).ThenInclude(s => s.TimeSlot)
                                    .Include(a => a.PatientRecords)
                                    .Include(a => a.Specialty)
                                    .Where(a => a.Schedule.DentistID == dentist.ID)
                                    .ToListAsync();
            ViewBag.DentistAvatar = dentist?.Account.Image ?? "https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Profile%2FPatient%2Fuser.png?alt=media&token=9010a4a6-0220-4d29-bb85-1fe425100744";
            ViewBag.DentistName = dentist?.Account.LastName + " " + dentist?.Account.FirstName;
            TempData["ErrorMessage"] = TempData["ErrorMessage"] as string;
            TempData["SuccessMessage"] = TempData["SuccessMessage"] as string;
            return View(appointments);
        }




        //[HttpPost]
        //public async Task<IActionResult> ChangePatientAppointment(int appointmentID, string appointmentStatus)
        //{
        //    var dentistAccountID = HttpContext.Session.GetInt32("dentistAccountID");
        //    if (dentistAccountID == null)
        //    {
        //        return RedirectToAction("login", "dentistaccount", new { area = "dentist" });
        //    }
        //    var appointment = await _context.Appointments.Include(a => a.Transactions).Where(a => a.ID == appointmentID).FirstOrDefaultAsync();
        //    appointment.AppointmentStatus = appointmentStatus;

        //    _context.Update(appointment);
        //    await _context.SaveChangesAsync();

        //    var schedule = await _context.Appointments.Include(s => s.Schedule).FirstOrDefaultAsync(a => a.ID == appointmentID);
        //    if (appointment.AppointmentStatus == "Đã Khám" || appointment.AppointmentStatus == "Đã Hủy")
        //    {
        //        schedule.Schedule.ScheduleStatus = "Còn Trống";
        //        await _context.SaveChangesAsync();
        //    }

        //    // HERE
        //    #region Refund MOMO API
        //    /*if (appointment.AppointmentStatus == "Đã Khám")
        //    {
        //        // Invoke the refund method from payment controller
        //        // Hoàn tiền thành công
        //        //return View("RefundSuccess", responseObject);
        //        var transactionCode = appointment.Transactions.FirstOrDefault()?.TransactionCode;
        //        var amount = appointment.Transactions.FirstOrDefault()?.TotalPrice;
        //        var bankName = appointment.Transactions.FirstOrDefault()?.BankName;
        //        var fullName = appointment.Transactions.FirstOrDefault()?.FullName;
        //        if (_momoPayment.RefundPayment((long)decimal.Parse(amount.ToString()), long.Parse(transactionCode.ToString()), "") != null)
        //        {
        //            TempData["RefundMessage"] = "Hoàn tiền thành công";

        //            var transaction = new Transaction
        //            {
        //                AppointmentID = appointment.ID,
        //                Date = DateTime.Now,
        //                BankName = bankName,
        //                TransactionCode = transactionCode,
        //                PaymentMethod = "MOMO",
        //                TotalPrice = amount,
        //                BankAccountNumber = "9704198526191432198",
        //                FullName = fullName,
        //                Message = "Hoàn tiền thành công",
        //                Status = "Thành Công"
        //            };

        //            _context.Transactions.Add(transaction);
        //            _context.SaveChanges();
        //        }

        //    }*/
        //    #endregion

        //    TempData["message"] = "success";
        //    return RedirectToAction("PatientAppointments");
        //}
        #endregion

        #region Hàm xử lý nút bấm thay đổi trạng thái đơn khám của bệnh nhân với dentist tương ứng
        [HttpGet]
        //Hàm hủy đơn đặt
        public async Task<IActionResult> CancelAppointment(int appointmentID, string? description)  //string description
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.ID == appointmentID && (a.AppointmentStatus == "Đã Chấp Nhận" || a.AppointmentStatus == "Chờ Xác Nhận"));
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
                return RedirectToAction("patientappointments");
            }
            appointment.AppointmentStatus = "Đã Hủy";
            appointment.Description = "Đã hủy. Lý do hủy: " + description;
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
            return RedirectToAction("patientappointments");
        }

        //Hàm thay đổi trạng thái của đơn đặt
        public async Task<IActionResult> ChangeStatusAppointment(int appointmentID, int statusNumber)
        {
            var appointment = _context.Appointments.FirstOrDefault(a => a.ID == appointmentID && (a.AppointmentStatus == "Chờ Xác Nhận" || a.AppointmentStatus == "Đã Chấp Nhận"));
            if (appointment == null)
            {
                TempData["ErrorMessage"] = "Lỗi! Không tìm thấy đơn đặt tương ứng hoặc trạng thái không hợp lệ.";
                return RedirectToAction("patientappointments");
            }

            if (statusNumber == 1)
            {
                appointment.AppointmentStatus = "Đã Chấp Nhận";
            }
            else if (statusNumber == 2)
            {
                appointment.AppointmentStatus = "Đã Khám";
            }
            _context.Update(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thay đổi trạng thái thành công!";
            return RedirectToAction("patientappointments");
        }
        #endregion

    }
}
