using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class ClinicCurrentController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public ClinicCurrentController(DentalClinicDbContext context)
        {
            _context = context;
        }

        #region Show List Clinic
        //===================LIST CLINIC===================
        [Route("ListClinic")]
        public async Task<IActionResult> ListClinic()
        {
            //Lấy dữ liệu từ cơ sở dữ liệu
            var clinics = await (from clinic in _context.Clinics
                                 join account in _context.Accounts
                                 on clinic.ManagerID equals account.ID

                                 where account.Role == "Quản Lý" && clinic.ClinicStatus == "Hoạt Động"
                                 select new ManagerClinicVM
                                 {
                                     ClinicName = clinic.Name,
                                     //Address = clinic.Address,
                                     Province = clinic.Province,
                                     Image = clinic.Image,
                                     Id = clinic.ID,
                                     ManagerName = account.LastName + " " + account.FirstName
                                 }).ToListAsync();

            return View(clinics);
        }
        #endregion

        #region Tìm kiếm phòng khám (Search)
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
                        where account.Role == "Quản Lý" && clinic.ClinicStatus == "Hoạt Động"
                        select new ManagerClinicVM
                        {
                            ClinicName = clinic.Name,
                            //Address = clinic.Address,
                            Province = clinic.Province,
                            Image = clinic.Image,
                            ManagerName = account.LastName + " " + account.FirstName
                        };

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ClinicName.Contains(search));
            }

            var clinics = await query.ToListAsync();

            var clinicList = clinics.Select(c => new ManagerClinicVM
            {
                ClinicName = c.ClinicName,
                //Address = c.Address,
                Province = c.Province,
                Image = c.Image,
                ManagerName = c.ManagerName
            }).ToList();

            return View(nameof(ListClinic), clinics);
        }
        #endregion

        #region Chỉnh sửa (Edit Clinic) [Đang suy xét giữa Edit và View]
        //===================CHỈNH SỬA PHÒNG KHÁM===================
        [HttpGet]
        [Route("EditClinic/{id}")]
        public async Task<IActionResult> EditClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            var model = new AddClincVM
            {
                ID = clinic.ID,
                Name = clinic.Name,
                PhoneNumber = clinic.PhoneNumber,
                Email = clinic.Email,
                ManagerID = clinic.ManagerID,
                Province = clinic.Province,
                District = clinic.District,
                Ward = clinic.Ward,
                Basis = clinic.Basis,
                Address = clinic.Address,
                Description = clinic.Description,
                Image = clinic.Image,
                ClinicStatus = "Hoạt Động",
                UnassignedManagers = new SelectList(await _context.Accounts
                    .Where(a => a.Role == "Quản Lý" && (!_context.Clinics.Any(c => c.ManagerID == a.ID) || a.ID == clinic.ManagerID))
                    .Select(a => new
                    {
                        a.ID,
                        FullName = a.LastName + " " + a.FirstName
                    }).ToListAsync(), "ID", "FullName")
            };

            return View(model);
        }

        [HttpPost]
        [Route("EditClinic")]
        public async Task<IActionResult> EditClinic(AddClincVM model)
        {
            if (ModelState.IsValid)
            {
                ////Kiểm tra Tên phòng khám đã tồn tại chưa
                //bool existingName = await _context.Clinics.AnyAsync(c => c.Name == model.Name);
                //if(existingName)
                //	ModelState.AddModelError("Name", "Tên phòng khám đã tồn tại.");

                ////Kiểm tra Email đã tồn tại chưa
                //bool existingEmail = await _context.Clinics.AnyAsync(c => c.Email == model.Email);
                //if (existingEmail)
                //	ModelState.AddModelError("Email", "Email đã tồn tại.");

                ////Kiểm tra Số điện thoại đã tồn tại chưa
                //bool existingPhone = await _context.Clinics.AnyAsync(c => c.PhoneNumber == model.PhoneNumber);
                //if (existingPhone)
                //	ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại.");

                //            //Nếu đã tồn tại, load lại danh sách người quản lý chưa có gắn phòng khám
                //if(!ModelState.IsValid)
                //{
                //                var unassignedManager = await _context.Accounts
                //                    .Where(a => a.Role == "Quản lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
                //                    .Select(a => new
                //                    {
                //                        a.ID,
                //                        FullName = a.LastName + " " + a.FirstName
                //                    })
                //                    .ToListAsync();

                //                model.UnassignedManagers = new SelectList(unassignedManager, "ID", "FullName");
                //                return View(model);
                //            }

                var clinic = await _context.Clinics.FindAsync(model.ID);
                if (clinic == null)
                {
                    return NotFound();
                }

                clinic.Name = model.Name;
                clinic.PhoneNumber = model.PhoneNumber;
                clinic.Email = model.Email;
                clinic.ManagerID = model.ManagerID;
                clinic.Province = model.Province;
                clinic.District = model.District;
                clinic.Ward = model.Ward;
                clinic.Basis = model.Basis;
                clinic.Address = model.Address;
                clinic.Description = model.Description;
                clinic.Image = model.Image;
                clinic.ClinicStatus = "Hoạt Động";

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListClinic));
            }

            //Ghi lại List Manager chưa được chỉ định phòng khám nào
            model.UnassignedManagers = new SelectList(await _context.Accounts
                .Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
                .Select(a => new
                {
                    a.ID,
                    FullName = a.LastName + " " + a.FirstName
                }).ToListAsync(), "ID", "FullName");

            // Log model state errors
            //List<string> errorMessages = new List<string>();
            //foreach (var value in ModelState.Values)
            //{
            //	foreach (var error in value.Errors)
            //	{
            //		errorMessages.Add(error.ErrorMessage);
            //	}
            //}
            //string errorMessage = string.Join("\n", errorMessages);

            //return BadRequest(errorMessage);
            return View(model);
        }
        #endregion
        
        #region Đóng cửa phòng khám (Delete)
        //===================XÓA PHÒNG KHÁM===================
        [Route("HiddenClinic")]
        public async Task<IActionResult> HiddenClinic(string name, string status)
        {
            var clinic = await _context.Clinics.SingleOrDefaultAsync(c => c.Name == name);

            if (clinic != null)
            {
                clinic.ClinicStatus = status;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ListClinic));
        }
        #endregion

        //=====================PHÒNG KHÁM ĐÓNG CỬA=====================

        #region Show Clinic Closed
        [Route("ListClinicClosed")]
        public async Task<IActionResult> ListClinicClosed()
        {
            var clinicClosed = await (from clinic in _context.Clinics
                                      join account in _context.Accounts
                                      on clinic.ManagerID equals account.ID

                                      where account.Role == "Quản Lý" && clinic.ClinicStatus == "Đóng Cửa"
                                      select new ManagerClinicVM
                                      {
                                          ClinicName = clinic.Name,
                                          Province = clinic.Province,
                                          Id = clinic.ID,
                                          Image = clinic.Image,
                                          ManagerName = account.LastName + " " + account.FirstName
                                      }).ToListAsync();

            return View(clinicClosed);
            #endregion
        }

        #region Mở cửa lại phòng khám
        [Route("UnlockClinic")]
        public async Task<IActionResult> UnlockClinic(string name, string status)
        {
            var clinic = await _context.Clinics.SingleOrDefaultAsync(c => c.Name == name);

            if(clinic != null)
            {
                clinic.ClinicStatus = status;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ListClinicClosed));
        }
		#endregion
	}
}