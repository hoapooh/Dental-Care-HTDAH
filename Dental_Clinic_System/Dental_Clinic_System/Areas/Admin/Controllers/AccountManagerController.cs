//==============================================TÀI KHOẢN QUẢN LÝ================================================
using AutoMapper;
using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin")]
    //[Route("Admin/[controller]")]
    public class AccountManagerController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public AccountManagerController(DentalClinicDbContext context)
        {
            _context = context;
        }

        #region Show List Account với Role Quản lý
        //===================LIST ACCOUNT===================
        //[Route("ListAccountManager")]
        public async Task<IActionResult> ListAccountManager()
        {
            var accountsQuery = _context.Accounts
            .Where(a => a.AccountStatus == "Hoạt Động" && a.Role == "Quản Lý");

            var accounts = await accountsQuery.ToListAsync();

            var accountList = accounts.Select(a => new ManagerAccountVM
            {
                Id = a.ID,
                Username = a.Username,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Address = a.Address,
                Role = a.Role
                //Status = a.AccountStatus
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
                return RedirectToAction(nameof(ListAccountManager));
            }

            var accountQuesry = _context.Accounts
            .Where(a => a.Username.Contains(keyword) && a.AccountStatus == "Hoạt Động" && a.Role == "Quản Lý");

            var account = await accountQuesry.Where(a => a.Username.Contains(keyword)).ToListAsync();

            var accountList = account.Select(a => new ManagerAccountVM
            {
                Id = a.ID,
                Username = a.Username,
                Email = a.Email,
                PhoneNumber = a.PhoneNumber,
                Address = a.Address,
                Role = a.Role
                //Status = a.AccountStatus
            }).ToList();

            return View("ListAccountManager", accountList);
        }
        #endregion

        #region Thêm tài khoản (Add Account)
        //===================THÊM TÀI KHOẢN===================
        //[Route("AddAccount")]
        [HttpPost]
        public async Task<IActionResult> AddAccount(string username, string password, string phoneNumber, string email, string address, string role)
        {
            //Check thông tin trùng lặp
            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == email || a.PhoneNumber == phoneNumber || a.Username == username);

            if (existingAccount != null)
            {
                //Thấy thông tin bị trùng, thông báo lỗi
                ModelState.AddModelError(string.Empty, "Thông tin người dùng đã tồn tại.");

                //Lấy lại list account để hiển thị
                var accounts = await _context.Accounts
                    .Where(a => a.AccountStatus == "Hoạt Động" && a.Role == "Quản Lý")
                    .ToListAsync();

                var accountList = accounts.Select(a => new ManagerAccountVM
                {
                    Id = a.ID,
                    Username = a.Username,
                    Email = a.Email,
                    PhoneNumber = a.PhoneNumber,
                    Address = a.Address,
                    Role = a.Role
                }).ToList();

                return View("ListAccountManager", accountList);
            }

            //Thêm mới account vào DB
            var account = new Account
            {
                Username = username,
                Password = DataEncryptionExtensions.ToMd5Hash(password),
                PhoneNumber = phoneNumber,
                Email = email,
                Address = address,
                Role = "Quản Lý",
                AccountStatus = "Hoạt Động"
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ListAccountManager));
        }
        #endregion

        #region Chỉnh sửa (Edit Account)
        //===================CHỈNH SỬA TÀI KHOẢN===================

        //[Route("EditAccount/{id}")]
        public async Task<IActionResult> EditAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            var accountVM = new EditAccountVM
            {
                Id = account.ID,
                FirstName = account.FirstName,
                LastName = account.LastName,
                Username = account.Username,
                Password = DataEncryptionExtensions.ToMd5Hash(account.Password),
                Email = account.Email,
                DateOfBirth = account.DateOfBirth,
                PhoneNumber = account.PhoneNumber,
                Gender = account.Gender,
                Province = account.Province,
                District = account.District,
                Ward = account.Ward,
                Address = account.Address,
                Role = account.Role
            };

            return View(accountVM);
        }

        [HttpPost]
        //[Route("EditAccount")]
        public async Task<IActionResult> EditAccount(EditAccountVM model)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.Accounts.FindAsync(model.Id);
                if (account == null)
                {
                    return NotFound();
                }

                account.ID = model.Id;
                account.FirstName = model.FirstName;
                account.LastName = model.LastName;
                account.Username = model.Username;
                account.Password = DataEncryptionExtensions.ToMd5Hash(model.Password);
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
                await _context.SaveChangesAsync();

                //Chuyển đến ListAccount
                return RedirectToAction(nameof(ListAccountManager));
            }

            return View(model);
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
            return RedirectToAction(nameof(ListAccountManager));
        }
        #endregion
    }
}
