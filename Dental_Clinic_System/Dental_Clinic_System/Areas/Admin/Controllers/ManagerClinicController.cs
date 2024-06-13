using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class ManagerClinicController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public ManagerClinicController(DentalClinicDbContext context)
        {
            _context = context;
        }

        //===================LIST ACCOUNT===================
        [Route("ListClinic")]
        public async Task<IActionResult> ListClinic()
        {
            //Lấy dữ liệu từ cơ sở dữ liệu
            var clinics = await (from clinic in _context.Clinics
                                 join account in _context.Accounts
                                 on clinic.ManagerID equals account.ID
                                 where account.Role == "Quản lý" && clinic.ClinicStatus == "Hoạt động"
                                 select new ManagerClinicVM
                                 {
                                     ClinicName = clinic.Name,
                                     Address = clinic.Address,
                                     ManagerName = account.LastName + " " + account.FirstName
                                 }).ToListAsync();

            return View(clinics);
        }


        //===================TÌM KIẾM===================
        [Route("SearchClinic")]
        public async Task<IActionResult> SearchClinic(string search)
        {
            //Nếu search rỗng, sẽ chuyển hướng đến ListClinic
            if (string.IsNullOrEmpty(search))
            {
                return RedirectToAction(nameof(ListClinic));
            }

            var query = from clinic in _context.Clinics
                        join account in _context.Accounts
                        on clinic.ManagerID equals account.ID
                        where account.Role == "Quản lý" && clinic.ClinicStatus == "Hoạt động"
						select new ManagerClinicVM
                        {
                            ClinicName = clinic.Name,
                            Address = clinic.Address,
                            ManagerName = account.Username
                        };

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ClinicName.Contains(search));
            }

            var clinics = await query.ToListAsync();

            var clinicList = clinics.Select(c => new ManagerClinicVM
            {
                ClinicName = c.ClinicName,
                Address = c.Address,
                ManagerName = c.ManagerName
            }).ToList();

            return View(nameof(ListClinic), clinics);
        }

        //===================THÊM PHÒNG KHÁM===================
        [Route("CreateClinic")]
        public IActionResult CreateClinic()
        {
			//Tùy chọn down bất kỳ dữ liệu nào cần thiết cho biểu mẫu
			ViewData["Managers"] = new SelectList(_context.Accounts.Where(a => a.Role == "Quản lý"), "ID", "Username");
            return View();
        }

        
        [HttpPost]
        [Route("CreateClinic")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClinic(AddClincVM model)
        {
            if (ModelState.IsValid)
            {
                var clinic = new Clinic
                {
                    Name = model.Name,
                    Province = model.Province,
                    District = model.District,
                    Ward = model.Ward,
                    Address = model.Address,
                    Basis = model.Basis,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Description = model.Description,
                    Image = model.Image,
                    ClinicStatus = model.ClinicStatus,
                    //MapLinker = model.MapLinker,
                    ManagerID = model.ManagerID
                };

                _context.Add(clinic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If the model state is invalid, reload the form with the existing data
            ViewData["Managers"] = new SelectList(_context.Accounts.Where(a => a.Role == "Quản lý"), "ID", "Username");
            return View(model);
        }

        //===================XÓA PHÒNG KHÁM===================
        [Route("HiddenClinic")]
        public async Task<IActionResult> HiddenClinic(string name, string status)
        {
            var clinic = await _context.Clinics.SingleOrDefaultAsync(c => c.Name == name);

            if(clinic != null)
            {
                clinic.ClinicStatus = status;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ListClinic));
        }
	}
}