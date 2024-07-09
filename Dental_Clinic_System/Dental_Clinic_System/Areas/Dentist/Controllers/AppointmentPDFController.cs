using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services;
using Dental_Clinic_System.Helper;
using Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using System.Globalization;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
    [Area("Dentist")]
    public class AppointmentPdfController : Controller
    {
        private readonly PdfService _pdfService;
        private readonly DentalClinicDbContext _context;
        IWebHostEnvironment _webHostEnvironment;

        public AppointmentPdfController(PdfService pdfService, DentalClinicDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _pdfService = pdfService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public IActionResult GeneratePdf(int appointmentID, string? ketquakham, string? hentaikham, string? dando)
        {
            var appointment = _context.Appointments
                .Include(a => a.Schedule)
                    .ThenInclude(s => s.Dentist)
                    .ThenInclude(d => d.Clinic)
                    .ThenInclude(c => c.Manager)
                .Where(a => a.ID == appointmentID)
                .Select(a => new {
                    a, //lấy tất cả thông tin của appointment
                    // Phần Bệnh Nhân:
                    a.PatientRecords.FullName,
                    a.PatientRecords.Gender,
                    a.PatientRecords.PhoneNumber,
                    a.PatientRecords.EmailReceiver,
                    Province = LocalAPIReverseString.GetProvinceNameById(a.PatientRecords.Province).Result,
                    District = LocalAPIReverseString.GetDistrictNameById(a.PatientRecords.Province, a.PatientRecords.District).Result,
                    Ward = LocalAPIReverseString.GetWardNameById(a.PatientRecords.District, a.PatientRecords.Ward).Result,
                    a.PatientRecords.Address,
                    AppointmentDate = a.Schedule.Date.ToString("dd/MM/yyyy"), 

                    // Phần Phòng Khám + Nha Sĩ:
                    DentistID = a.Schedule.Dentist.ID,
                    Specialty = a.Schedule.Dentist.DentistSpecialties.First().Specialty.Name,
                    ClinicName = a.Schedule.Dentist.Clinic.Name,
                    ClinicProvince = LocalAPIReverseString.GetProvinceNameById(a.Schedule.Dentist.Clinic.Province ?? 0).Result,
                    ClinicDistrict = LocalAPIReverseString.GetDistrictNameById(a.Schedule.Dentist.Clinic.Province ?? 0, a.Schedule.Dentist.Clinic.District ?? 0).Result,
                    ClinicWard = LocalAPIReverseString.GetWardNameById(a.Schedule.Dentist.Clinic.District ?? 0, a.Schedule.Dentist.Clinic.Ward ?? 0).Result,
                    ClinicAddress= a.Schedule.Dentist.Clinic.Address,
                    ClinicEmail =a.Schedule.Dentist.Clinic.Manager.Email,
                    ClinicPhone = a.Schedule.Dentist.Clinic.Manager.PhoneNumber,
                    DentistName = a.Schedule.Dentist.Account.LastName + " " + a.Schedule.Dentist.Account.FirstName,
                    DentistPhoneNumber = a.Schedule.Dentist.Account.PhoneNumber


                })
                .FirstOrDefault();

            if (appointment == null)
            {
                return NotFound();
            }

            string logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "avatar.jpg").Replace("\\", "/");

            string formattedDate = DateTime.Now.ToString("dd 'tháng' MM 'năm' yyyy", new System.Globalization.CultureInfo("vi-VN"));

            //Lấy ngày và giờ riêng ra của hentaikham
            FormatDateTime(hentaikham, out DateOnly desiredDate, out TimeOnly startTime, out TimeOnly endTime);

            //PHẦN TẠO NÔI DUNG HTML CHO PDF
            string htmlContent = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>Appointment_PDF</title>
</head>
<body>
    <div class=""header__top"">
        <div class=""clinic__info"">
            <p><strong>Phòng Khám: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicName}</span></p>
            <p><strong>Địa Chỉ: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicAddress}, {appointment.ClinicWard}, {appointment.ClinicDistrict}, {appointment.ClinicProvince}</span></p>
            <p><strong>Điện Thoại: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicPhone}</span></p>
            <p><strong>Email: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicEmail}</span></p>
        </div>
    </div>
    <div class=""title__top"">
        <p style=""margin-right: 40%;"">Mã Số Phiếu: {appointmentID}</p>
        <p>{appointment.ClinicProvince}, {formattedDate}</p>
    </div><br><br><br>
    <h2 style='text-align:center;'>Phiếu Khám Nha Khoa</h2>
    <div class=""personal__info"">
        <div class=""w-40"" style=""margin-right:20%;"">
            <p><strong>Họ và Tên: </strong> {appointment.FullName} </p>
            <p><strong>Giới Tính: </strong> {appointment.Gender} </p>
            <p><strong>Địa Chỉ: </strong> {appointment.Address}, {appointment.Ward}, {appointment.District}, {appointment.Province} </p>
            <p><strong>Điện Thoại: </strong> {appointment.PhoneNumber} </p>
            <p><strong>Email: </strong> {appointment.EmailReceiver} </p>
        </div>
        <div class=""w-40"">
            <p><strong>Nha Sĩ Khám: </strong> {appointment.DentistName} </p>
            <p><strong>Số Điện Thoại Liên Hệ: </strong> {appointment.DentistPhoneNumber} </p>
            <p><strong>Đơn Vị Tiếp Nhận Đặt Khám: </strong> Dental Care </p>
        </div>
    </div>
    <table>
        <tr>
            <th>STT</th>
            <th>Tên Chuyên Khoa</th>
            <th>Ngày Khám</th>
            <th>Số Lượng</th>
            <th>Giá Khám</th>
        </tr>
        <tr>
            <td>1</td>
            <td>{appointment.Specialty}</td>
            <td>{appointment.AppointmentDate}</td>
            <td>1</td>
            <td>{string.Format(new CultureInfo("vi-VN"), "{0:#,##0.} đ", appointment.a.TotalPrice)}</td>
        </tr>
    </table>
    <p><strong>Tổng chi phí: </strong> {string.Format(new CultureInfo("vi-VN"), "{0:#,##0.} đ", appointment.a.TotalPrice)}</p>
    <p><strong>Tình Trạng: </strong>{appointment.a.AppointmentStatus}</p>
    <p><strong>Kết Quả Khám: </strong>{ketquakham}</p>
    <p><strong>HẸN TÁI KHÁM (nếu có): </strong>{startTime.ToString("HH:mm")}-{endTime.ToString("HH:mm")} {desiredDate.ToString("dd/MM/yyyy")}</p>
    <p><strong>Dặn dò (nếu có): </strong>{dando}</p> <br><br><br><br><br><br>
    <div class=""footer__signature"">
        <p style=""margin-left:10%; margin-right: 50%;""><strong>Bệnh Nhân</strong> <br>(Ký và ghi rõ họ tên)</p>
        <p><strong>Nha Sĩ</strong> <br>(Ký và ghi rõ họ tên)</p>
    </div> <br><br><br><br><br>
</body>
</html>
";


            byte[] pdf = _pdfService.GeneratePdf(htmlContent);
            string fileName = $"donkham_{appointmentID}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", fileName);

            System.IO.File.WriteAllBytes(filePath, pdf);

            //Thêm appointment với giờ tạo vào FutureAppointments =====================
            if (appointment.a.Future_Appointment_ID == null) { 
                if (!string.IsNullOrEmpty(hentaikham))
                {
                    appointment.a.Note = $"Dặn dò: {dando}";
                    appointment.a.Description = $"Đã Khám. Kết quả Khám: {ketquakham}";
                    FutureAppointment futureAppointment = new()
                    {
                        PatientRecord_ID = appointment.a.PatientRecordID,
                        Dentist_ID = appointment.DentistID,
                        StartTime = startTime,
                        EndTime = endTime,
                        DesiredDate = desiredDate,
                        FutureAppointmentStatus = "Chưa Khám"
                    };
                    _context.FutureAppointments.Add(futureAppointment);
                    _context.SaveChanges();
                    appointment.a.Future_Appointment_ID = futureAppointment.ID;
                    _context.SaveChanges();
                }
            }
            else if(appointment.a.Future_Appointment_ID != null)
            {
                if (!string.IsNullOrEmpty(hentaikham))
                {
                    appointment.a.Note = $"Dặn dò: {dando}";
                    appointment.a.Description = $"Đã Khám. Kết quả Khám: {ketquakham}";
                    //Lấy FutureAppointment để cập nhật lại thời gian tái khám
                    var futureAppointment = _context.FutureAppointments.Find(appointment.a.Future_Appointment_ID);
                    if (futureAppointment == null) return NotFound("Không tìm thấy lịch định kỳ");

                    futureAppointment.DesiredDate = desiredDate;
                    futureAppointment.StartTime = startTime;
                    futureAppointment.EndTime = endTime;
                    _context.SaveChanges();
                }
            }
            //=========================================================================

            //return File(pdf, "application/pdf", "appointment.pdf");
            return Redirect($"/pdf/{fileName}");
        }

        private void FormatDateTime(string dateTime, out DateOnly desiredDate ,out TimeOnly startTime, out TimeOnly endTime)
        {
            string[] periodicDateTime = dateTime.Split("T");
            desiredDate = DateOnly.ParseExact(periodicDateTime[0], "MM/dd/yyyy", CultureInfo.InvariantCulture);
            startTime = TimeOnly.ParseExact(periodicDateTime[1], "HH:mm");
            endTime = startTime.AddMinutes(30);
        }
    }
}
