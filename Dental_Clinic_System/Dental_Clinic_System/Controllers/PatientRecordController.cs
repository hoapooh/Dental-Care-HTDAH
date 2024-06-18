using Dental_Clinic_System.Areas.Manager.ViewModels;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Dental_Clinic_System.Controllers
{
	[Authorize(Roles = "Bệnh Nhân")]
	public class PatientRecordController : Controller
	{
		private readonly DentalClinicDbContext _context;

		public PatientRecordController(DentalClinicDbContext context)
		{
			_context = context;
		}

		//=============================== PHẦN XỬ LÝ =========================================

		[HttpGet]
		public async Task<IActionResult> PatientRecord(int clinicID, int specialtyID, int dentistID, string scheduleID)
		{
            var claimsEmailValue = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var username = claimsEmailValue;
			var patientRecord = await _context.PatientRecords
										.Include(pr => pr.Account)
										.Include(pr => pr.Appointments)
										.Where(pr => pr.Account.Email == username)
										.ToListAsync();
			TempData["Patient Amount"] = patientRecord.Count;
			ViewBag.scheduleID = scheduleID;
			ViewBag.specialtyID = specialtyID;
			ViewBag.dentistID = dentistID;
			ViewBag.clinicID = clinicID;
			return View(patientRecord);
		}


		[HttpGet]
		[Route("/createnewpatientrecord")]
		public async Task<IActionResult> ShowFormCreatingNewPatientRecord()
		{

			return View("createnewpatientrecord");
		}

		#region Hàm nhận dữ liệu hồ sơ bệnh nhân (patient record) nếu chưa có hoặc tạo thêm hồ sơ bệnh nhân (thử nghiệm)
		[HttpPost]
		public async Task<IActionResult> CreateNewPatientRecord(PatientRecordVM record)
		{
            var claimsEmailValue = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = _context.Accounts.FirstOrDefault(u => u.Email == claimsEmailValue);
			if (user == null)
			{
                await Console.Out.WriteLineAsync("=========================================");
                await Console.Out.WriteLineAsync("NULLLLLLLLLLLLLLLLLLLLLLLLLL");
                await Console.Out.WriteLineAsync("===========================================================");

            }
            var age = DateTime.Now.Year - record.DateOfBirdth.Year;
			if(age >= 14)
			{
				ModelState.Remove(nameof(record.FMFullName));
				ModelState.Remove(nameof(record.FMEmail));
				ModelState.Remove(nameof(record.FMPhoneNumber));
				ModelState.Remove(nameof(record.FMRelationship));
				if(record.Province == 0 || string.IsNullOrEmpty(record.Province.ToString()))
				{
					ModelState.AddModelError(nameof(record.Province), "Vui lòng chọn tỉnh thành");
				}
				if (record.District == 0 || string.IsNullOrEmpty(record.District.ToString()))
				{
					ModelState.AddModelError(nameof(record.District), "Vui lòng chọn tỉnh thành");
				}
				if (record.Ward == 0 || string.IsNullOrEmpty(record.Ward.ToString()))
				{
					ModelState.AddModelError(nameof(record.Ward), "Vui lòng chọn tỉnh thành");
				}
			}
			else if (age < 14)
			{
				// Xóa các lỗi liên quan đến PhoneNumber, IdentifyNumber, Job
				ModelState.Remove(nameof(record.PhoneNumber));
				ModelState.Remove(nameof(record.IdentifyNumber));
				ModelState.Remove(nameof(record.Job));

				// Kiểm tra validation cho các trường FMFullName, FMRelationship, FMPhoneNumber, FMEmail
				if (string.IsNullOrWhiteSpace(record.FMFullName))
				{
					ModelState.AddModelError(nameof(record.FMFullName), "Vui lòng nhập đầy đủ họ tên nhân thân!");
				}
				if (string.IsNullOrWhiteSpace(record.FMRelationship))
				{
					ModelState.AddModelError(nameof(record.FMRelationship), "Vui lòng không bỏ trống quan hệ với bệnh nhân!");
				}
				if (string.IsNullOrWhiteSpace(record.FMPhoneNumber))
				{
					ModelState.AddModelError(nameof(record.FMPhoneNumber), "Vui lòng nhập số điện thoại nhân thân!");
				}
				if (string.IsNullOrWhiteSpace(record.FMEmail))
				{
					ModelState.AddModelError(nameof(record.FMEmail), "Vui lòng nhập email nhân thân!");
				}
			}
			else
			{
				// Xóa các lỗi liên quan đến FMFullName, FMRelationship, FMPhoneNumber, FMEmail
				ModelState.Remove(nameof(record.FMFullName));
				ModelState.Remove(nameof(record.FMRelationship));
				ModelState.Remove(nameof(record.FMPhoneNumber));
				ModelState.Remove(nameof(record.FMEmail));

				// Kiểm tra validation cho các trường PhoneNumber, IdentifyNumber, Job
				if (string.IsNullOrWhiteSpace(record.PhoneNumber))
				{
					ModelState.AddModelError(nameof(record.PhoneNumber), "Vui lòng nhập số điện thoại!");
				}
				if (string.IsNullOrWhiteSpace(record.IdentifyNumber))
				{
					ModelState.AddModelError(nameof(record.IdentifyNumber), "Vui lòng nhập mã định danh!");
				}
				if (string.IsNullOrWhiteSpace(record.Job))
				{
					ModelState.AddModelError(nameof(record.Job), "Vui lòng nhập nghề nghiệp!");
				}
			}

			if (ModelState.IsValid)
			{
				var patientrecord = new PatientRecord() 
				{ 
					FullName = record.FullName,
					AccountID = user.ID,
					DateOfBirth = record.DateOfBirdth,
					Gender = record.Gender,
					EmailReceiver = record.Email,
					PhoneNumber = record.PhoneNumber,
					Province = record.Province,
					District = record.District,
					Ward = record.Ward,
					Address = record.Address,
					IdentityNumber = record.IdentifyNumber,
					Job = record.Job,
					FMName = record.FMFullName,
					FMPhoneNumber = record.FMPhoneNumber,
					FMRelationship = record.FMRelationship,
				};

				 _context.PatientRecords.Add(patientrecord);
				 await _context.SaveChangesAsync();
                await Console.Out.WriteLineAsync("===========================================================");
                await Console.Out.WriteLineAsync("Everything seem be like good!");
                await Console.Out.WriteLineAsync("===========================================================");

                return RedirectToAction("Index", "Home"); // Chuyển hướng đến trang khác sau khi lưu thành công
			}

			return View(record); // Trả lại view với các lỗi validation nếu có
		}
		#endregion


		// Choose a patient record for booking (Method GET is here)
		[HttpGet]
		[Route("/appointment/confirm")]
		public async Task<IActionResult> ConfirmAppointment(int scheduleID, int patientRecordID, int specialtyID, int clinicID, int dentistID)
		{
			var appointment = new Dictionary<string, object>
            {
                { "Schedule", await _context.Schedules.Include(s => s.TimeSlot).FirstOrDefaultAsync(s => s.ID == scheduleID)},
                { "PatientRecord", await _context.PatientRecords.FirstOrDefaultAsync(pr => pr.ID == patientRecordID)},
                { "Specialty", await _context.Specialties.FirstOrDefaultAsync(sp => sp.ID == specialtyID)},
                { "Dentist", await _context.Dentists.Include(d => d.Account). FirstOrDefaultAsync(d => d.ID == dentistID)},
                { "Clinic", await _context.Clinics. FirstOrDefaultAsync(c => c.ID == clinicID) }
            };

            ViewBag.scheduleID = scheduleID;
			ViewBag.patientRecordID = patientRecordID;
			ViewBag.specialtyID = specialtyID;
			ViewBag.clinicID = clinicID;
			ViewBag.dentistID = dentistID;
            return View(appointment);
        }
        [HttpGet]
		public async Task<IActionResult> PatientRecordPaymentChoosing(int scheduleID, int patientRecordID, int specialtyID, int clinicID, int dentistID)
		{
            var appointment = new Dictionary<string, object>
            {
                { "Schedule", await _context.Schedules.Include(s => s.TimeSlot).FirstOrDefaultAsync(s => s.ID == scheduleID)},
                { "PatientRecord", await _context.PatientRecords.FirstOrDefaultAsync(pr => pr.ID == patientRecordID)},
                { "Specialty", await _context.Specialties.FirstOrDefaultAsync(sp => sp.ID == specialtyID)},
                { "Dentist", await _context.Dentists.Include(d => d.Account). FirstOrDefaultAsync(d => d.ID == dentistID)},
                { "Clinic", await _context.Clinics. FirstOrDefaultAsync(c => c.ID == clinicID) }
            };
            ViewBag.scheduleID = scheduleID;
			ViewBag.patientRecordID = patientRecordID;
			ViewBag.specialtyID = specialtyID;
			ViewBag.dentistID = dentistID;
			ViewBag.clinicID = clinicID;
            return View(appointment);
		}
	}
}
