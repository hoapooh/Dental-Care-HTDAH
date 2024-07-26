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
using Newtonsoft.Json;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using Dental_Clinic_System.Areas.Dentist.Helper;
using System.Text;

namespace Dental_Clinic_System.Areas.Dentist.Controllers
{
    [Area("Dentist")]
    public class AppointmentPdfController : Controller
    {
        //private readonly PdfService _pdfService;
        private readonly DentalClinicDbContext _context;
        IWebHostEnvironment _webHostEnvironment;

        //public AppointmentPdfController(PdfService pdfService, DentalClinicDbContext context, IWebHostEnvironment webHostEnvironment)
        //{
        //    _pdfService = pdfService;
        //    _context = context;
        //    _webHostEnvironment = webHostEnvironment;
        //}

        public AppointmentPdfController(DentalClinicDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult GeneratePdf(int dentistID, int appointmentID, string? ketquakham, List<DateOnly>? selectedDates, string? giobatdau, string? gioketthuc, string? dando)
        {
            #region Lấy thông tin appointment từ DB
            var appointment = _context.Appointments
                .Include(a => a.Schedule)
                    .ThenInclude(s => s.Dentist)
                    .ThenInclude(d => d.Clinic)
                    .ThenInclude(c => c.Manager)
                .Where(a => a.ID == appointmentID)
                .Select(a => new {
					//lấy tất cả thông tin của appointment
					a, 

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
					a.PatientRecords.FMEmail,
					a.PatientRecords.FMPhoneNumber,

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
                    DentistPhoneNumber = a.Schedule.Dentist.Account.PhoneNumber,

					//Transaction
					MedicalReport = a.Transactions.Where(t => t.AppointmentID == a.ID).Select(t => t.MedicalReportID).FirstOrDefault()
                })
                .FirstOrDefault();
            #endregion


            if (appointment == null)
            {
                return Redirect("/Hone/Error");
            }

            string formattedDate = DateTime.Now.ToString("dd 'tháng' MM 'năm' yyyy", new System.Globalization.CultureInfo("vi-VN"));

            //Lấy ngày và giờ riêng ra của hentaikham
            AppointmentServices.FormatDateTime(selectedDates, giobatdau, gioketthuc ,out List<DateOnly> desiredDate, out TimeOnly startTime, out TimeOnly endTime);

			StringBuilder dateListBuilder = new StringBuilder();
			foreach (var date in selectedDates)
			{
				dateListBuilder.Append($"<li>{date.ToString("dd/MM/yyyy")}</li>");
			}

			#region PHẦN TẠO NÔI DUNG HTML CHO PDF
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
            <p><strong>{appointment.ClinicName}</strong></p>
            <p><strong>Địa Chỉ: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicAddress}, {appointment.ClinicWard}, {appointment.ClinicDistrict}, {appointment.ClinicProvince}</span></p>
            <p><strong>Điện Thoại: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicPhone}</span></p>
            <p><strong>Email: </strong><span style='font-size: 15px; color: black;'>{appointment.ClinicEmail}</span></p>
        </div>
    </div>
    <div class=""title__top"">
        <p style=""margin-right: 20%;"">Mã Số Phiếu: {appointment.MedicalReport}</p>
        <p>{appointment.ClinicProvince}, {formattedDate}</p>
    </div><br><br><br>
    <h2 style='text-align:center;'>Phiếu Khám Nha Khoa</h2>
    <div class=""personal__info"">
        <div class=""w-40"" style=""margin-right:20%;"">
            <p><strong>Họ và Tên: </strong> {appointment.FullName} </p>
            <p><strong>Giới Tính: </strong> {appointment.Gender} </p>
            <p><strong>Địa Chỉ: </strong> {appointment.Address}, {appointment.Ward}, {appointment.District}, {appointment.Province} </p>
            <p><strong>Điện Thoại Liên Hệ: </strong> {appointment.PhoneNumber ?? appointment.FMPhoneNumber} </p>
            <p><strong>Email Liên Hệ: </strong> {appointment.EmailReceiver ?? appointment.FMEmail} </p>
        </div>
        <div class=""w-40"">
            <p><strong>Nha Sĩ Khám: </strong> {appointment.DentistName} </p>
            <p><strong>Số Điện Thoại Liên Hệ: </strong> {appointment.DentistPhoneNumber} </p>
            <p><strong>Đơn Vị Tiếp Nhận Đặt Khám: </strong> Dental Care </p>
        </div>
    </div>
    <table>
        <tr style=""border-bottom: solid 1px;"">
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
    <p><strong>HẸN KHÁM ĐỊNH KỲ (nếu có): </strong>{startTime.ToString("HH:mm")}-{endTime.ToString("HH:mm")} <ul>Ngày Khám: {dateListBuilder.ToString()}</ul></p>
    <p><strong>Dặn dò (nếu có): </strong>{dando}</p> <br><br><br><br><br><br>
    <div class=""footer__signature"">
        <p style=""margin-left:10%; margin-right: 50%;""><strong>Bệnh Nhân</strong> <br>(Ký và ghi rõ họ tên)</p>
        <p><strong>Nha Sĩ</strong> <br>(Ký và ghi rõ họ tên)</p>
    </div> <br><br><br><br><br>
</body>
</html>
";
			#endregion

			//byte[] pdf = _pdfService.GeneratePdf(htmlContent);
            string fileName = $"donkham_{appointmentID}.pdf";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", fileName);

            //System.IO.File.WriteAllBytes(filePath, pdf);
			
			//return File(pdf, "application/pdf", "appointment.pdf"); //Trả về file pdf
			return Redirect($"/pdf/{fileName}");
        }   

    }
}
