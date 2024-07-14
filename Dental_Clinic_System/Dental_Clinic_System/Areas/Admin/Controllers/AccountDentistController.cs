//==============================================TÀI KHOẢN NHA SĨ================================================
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin,Mini Admin")]

    public class AccountDentistController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public AccountDentistController(DentalClinicDbContext context)
        {
            _context = context;
        }

        #region Show List Account với Role Nha Sĩ
        //===================LIST ACCOUNT===================
        //[Route("ListAccountDentist")]
        public async Task<IActionResult> ListAccountDentist()
        {
            var accounts = await _context.Accounts
            .Where(a => a.AccountStatus == "Hoạt Động" && a.Role == "Nha Sĩ")
            .ToListAsync();

            var accountList = accounts.Select(a => new ManagerAccountVM
            {

                Id = a.ID,
                Username = a.Username,
                Email = a.Email,
                FirstName = a.FirstName,
                LastName = a.LastName,
                PhoneNumber = a.PhoneNumber,
                Address = a.Address,
                Gender = a.Gender,
                //Role = a.Role,

                ClinicName = _context.Dentists
                    .Where(d => d.AccountID == a.ID)
                    .Select(d => d.Clinic.Name)
                    .FirstOrDefault(),

                Specialties = _context.DentistSpecialties
                    .Where(ds => ds.Dentist.AccountID == a.ID && ds.Check == true)
                    .Select(ds => ds.Specialty.Name)
                    .ToList()
            }).ToList();


            return View(accountList);
        }
        #endregion

        #region Tìm kiếm (Search)
        //===================TÌM KIẾM===================
        //[Route("SearchAccount")]
        public async Task<IActionResult> SearchAccount(string keyword)
        {
            //Nếu keyword rỗng, sẽ chuyển hướng đến ListAccount
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction(nameof(ListAccountDentist));
            }

            var accountQuesry = _context.Accounts
            .Where(a => a.Username.Contains(keyword) && a.AccountStatus == "Hoạt Động" && a.Role == "Nha Sĩ");

            var account = await accountQuesry.Where(a => a.Username.Contains(keyword)).ToListAsync();

            var accountList = account.Select(a => new ManagerAccountVM
            {
                Id = a.ID,
                Username = a.Username,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Address = a.Address,
                Gender = a.Gender,
                //Role = a.Role,

                ClinicName = _context.Dentists
                    .Where(d => d.AccountID == a.ID)
                    .Select(d => d.Clinic.Name)
                    .FirstOrDefault(),

                Specialties = _context.DentistSpecialties
                    .Where(ds => ds.Dentist.AccountID == a.ID && ds.Check == true)
                    .Select(ds => ds.Specialty.Name)
                    .ToList()
                
            }).ToList();

            return View("ListAccountDentist", accountList);
        }
        #endregion

        #region Thêm tài khoản (Add Account)
        [HttpGet]
        public async Task<IActionResult> GetDegrees()
        {
            var degrees = await _context.Degrees.ToListAsync();
            return Json(degrees);
        }

        [HttpGet]
        public async Task<IActionResult> GetClinics()
        {
            var clinic = await _context.Clinics.ToListAsync();
            return Json(clinic);
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecialties()
        {
            var specialties = await _context.Specialties.ToListAsync();
            return Json(specialties);
        }

        //===================THÊM TÀI KHOẢN===================
        [HttpPost]
        public async Task<IActionResult> AddAccount(string username, string password, string phoneNumber, string email, string gender, string firstname, string lastname, int degreeID, int clinicID, List<int> selectSpecialty)
        {
            //Check thông tin trùng lặp
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == email || a.PhoneNumber == phoneNumber || a.Username == username);

            if (existingAccount != null)
            {
                //Kiểm tra tên đăng nhập đã tồn tại chưa
                bool accountName = await _context.Accounts.AnyAsync(a => a.Username == username);
                if (accountName)
                {
                    ModelState.AddModelError("Name", "Tên đăng nhập đã tồn tại");
                }

                //Kiểm tra sđt đã tồn tại chưa
                bool accountPhoneNumber = await _context.Accounts.AnyAsync(a => a.PhoneNumber == phoneNumber);
                if (accountPhoneNumber)
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại");
                }

                //Kiểm tra Email đã tồn tại chưa
                bool accountEmail = await _context.Accounts.AnyAsync(a => a.Email == email);
                if (accountEmail)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                }

                //Lấy lại list account để hiển thị
                var account1 = await _context.Accounts
                    .Where(a => a.AccountStatus == "Hoạt Động" && a.Role == "Nha Sĩ")
                    .ToListAsync();

                var accountList = account1.Select(a => new ManagerAccountVM
                {
					Id = a.ID,
					FirstName = a.FirstName,
					LastName = a.LastName,
					Username = a.Username,
					Email = a.Email,
					PhoneNumber = a.PhoneNumber,
					Address = a.Address,
					Gender = a.Gender,
					//Role = a.Role,

					ClinicName = _context.Dentists
					.Where(d => d.AccountID == a.ID)
					.Select(d => d.Clinic.Name)
					.FirstOrDefault(),

					Specialties = _context.DentistSpecialties
					.Where(ds => ds.Dentist.AccountID == a.ID && ds.Check == true)
					.Select(ds => ds.Specialty.Name)
					.ToList()
				}).ToList();

                return View("ListAccountDentist", accountList);
            }

            //Thêm mới Account vào DB
            var account = new Account
            {
                Username = username,
                FirstName = firstname,
                LastName = lastname,
                Password = DataEncryptionExtensions.ToMd5Hash(password),
                PhoneNumber = phoneNumber,
                Email = email,
                Gender = gender,
                Role = "Nha Sĩ",
                AccountStatus = "Hoạt Động"
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            //Thêm mới Dentist vào DB
            var newDentist = new Dental_Clinic_System.Models.Data.Dentist
            {
                AccountID = account.ID,
                ClinicID = clinicID,
                DegreeID = degreeID,
            };
            _context.Dentists.Add(newDentist);
            await _context.SaveChangesAsync();

            //Thêm mới Dentist có nhiều Specialty (Chuyên khoa)
            var specialties = await _context.Specialties.ToListAsync();
            foreach (var s in specialties)
            {
                var dentistSpecialty = new DentistSpecialty
                {
                    DentistID = newDentist.ID,
                    SpecialtyID = s.ID,
                    Check = selectSpecialty.Contains(s.ID)
                };
                _context.DentistSpecialties.AddAsync(dentistSpecialty);
            }
            await _context.SaveChangesAsync();

            var dentists = await _context.Dentists
                .Include(d => d.Account)
                .Include(d => d.Clinic)
                .Include(d => d.Degree)
                .ToListAsync();

            TempData["ToastMessageSuccessTempData"] = "Đăng ký tài khoản nha sĩ thành công";
            return RedirectToAction(nameof(ListAccountDentist), dentists);
        }
        #endregion

        #region Chỉnh sửa (Edit Account)
        //===================CHỈNH SỬA TÀI KHOẢN===================
        [HttpGet]
        public async Task<IActionResult> EditAccountDentist(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                Console.WriteLine($"Account với ID {id} không tìm thấy.");
                return NotFound();
            }

            var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.AccountID == id);
            if (dentist == null)
            {
                Console.WriteLine($"Dentist với AccountID {id} không tìm thấy.");
                return NotFound();
            }

            var selectSpecialty = await _context.DentistSpecialties
                .Where(ds => ds.DentistID == dentist.ID && ds.Check == true)
                .Select(ds => ds.SpecialtyID)
                .ToListAsync();

            var accountVM = new EditAccountDentistVM
            {
                Id = account.ID,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Username = account.Username,
                Email = account.Email,
                DateOfBirth = account.DateOfBirth,
                PhoneNumber = account.PhoneNumber,
                Gender = account.Gender,
                Province = account.Province,
                District = account.District,
                Ward = account.Ward,
                Address = account.Address,
                Role = account.Role,
                DegreeID = dentist.DegreeID,
                ClinicID = dentist.ClinicID,
                SelectSpecialty = selectSpecialty,
            };

            ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", dentist.DegreeID);
            ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "Name", dentist.ClinicID);
            ViewData["Specialties"] = await _context.Specialties.ToListAsync();
            return View(accountVM);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccountDentist(EditAccountDentistVM model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.Accounts.FindAsync(model.Id);
                if (account == null)
                {
                    return NotFound();
                }

                //Kiểm tra tên đăng nhập đã tồn tại chưa
                if (await _context.Accounts.AnyAsync(a => a.Username == model.Username && a.ID != model.Id))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");

                    //Đặt lại ViewData trước khi trả về View
                    SetViewData(model);
                    return View(model);
                }

                //Kiểm tra Sđt đã tồn tại chưa
                if (await _context.Accounts.AnyAsync(a => a.PhoneNumber == model.PhoneNumber && a.ID != model.Id))
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã tồn tại");
                    SetViewData(model);
                    return View(model);
                }

                //Kiểm tra Email đã tồn tại chưa
                if (await _context.Accounts.AnyAsync(a => a.Email == model.Email && a.ID != model.Id))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    SetViewData(model);
                    return View(model);
                }

                //Kiểm tra Mật khẩu mới có khớp với mật khẩu mới nhập lại không.
                if (!string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmPassword))
                {
                    if (model.NewPassword != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                        return View(model);
                    }

                    if (model.NewPassword.Length < 3)
                    {
                        ModelState.AddModelError("NewPassword", "Mật khẩu phải có ít nhất 3 ký tự.");
                        return View(model);
                    }

                    account.Password = DataEncryptionExtensions.ToMd5Hash(model.NewPassword);
                }

                account.ID = model.Id;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Username = model.Username;
                account.Email = model.Email;
                account.DateOfBirth = model.DateOfBirth;
                account.PhoneNumber = model.PhoneNumber;
                account.Gender = model.Gender;
                account.Province = model.Province;
                account.District = model.District;
                account.Ward = model.Ward;
                account.Address = model.Address;
                account.Role = model.Role;
                _context.Update(account);

                //Update nha sĩ entity
                var dentist = await _context.Dentists.FirstOrDefaultAsync(d => d.AccountID == model.Id);
                if (dentist != null)
                {
                    dentist.DegreeID = model.DegreeID;
                    dentist.ClinicID = model.ClinicID;
                    _context.Update(dentist);

                    var existSpecialty = await _context.DentistSpecialties.Where(ds => ds.DentistID == dentist.ID).ToListAsync();
                    _context.DentistSpecialties.RemoveRange(existSpecialty);    //Xóa các chuyên khoa hiện có

                    var newSpecialty = _context.Specialties.ToList().Select(s => new DentistSpecialty
                    {
                        DentistID = dentist.ID,
                        SpecialtyID = s.ID,
                        Check = model.SelectSpecialty.Contains(s.ID)
                    });
                    _context.DentistSpecialties.AddRange(newSpecialty);
                }

                await _context.SaveChangesAsync();

                TempData["ToastMessageSuccessTempData"] = "Chỉnh sửa tài khoản nha sĩ thành công";
                return RedirectToAction(nameof(ListAccountDentist));
            }

			//Load lại dữ liệu khi ModelState không hợp lệ
			ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", model.DegreeID);
            ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "Name", model.ClinicID);
			TempData["ToastMessageFailTempData"] = "Chỉnh sửa tài khoản nha sĩ thất bại";
			return View(model);
        }
        
        //Đặt lại ViewData
        private void SetViewData(EditAccountDentistVM model)
        {
            ViewData["DegreeID"] = new SelectList(_context.Degrees, "ID", "Name", model.DegreeID);
            ViewData["ClinicID"] = new SelectList(_context.Clinics, "ID", "Name", model.ClinicID);
            ViewData["Specialties"] = _context.Specialties.ToList();
        }
        #endregion

        #region Tài khoản bị khóa (Delete)
        //===================KHÓA TÀI KHOẢN===================
        //[Route("HiddenAccountStatus")]
        [HttpPost]
        public async Task<IActionResult> HiddenAccountStatus(string username, string status)
        {
            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);

            if (account != null)
            {
                account.AccountStatus = status;
                await _context.SaveChangesAsync();
            }
            TempData["ToastMessageSuccessTempData"] = "Cấm tài khoản nha sĩ thành công";
            return RedirectToAction(nameof(ListAccountDentist));
        }
        #endregion
    }
}
