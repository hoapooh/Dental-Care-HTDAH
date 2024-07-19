using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Dental_Clinic_System.Helper;

namespace Dental_Clinic_System.Controllers
{
    [Area("Manager")]
	// [Route("Manager/[controller]")]
	[Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
	public class ServiceController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public ServiceController(DentalClinicDbContext context)
        {
            _context = context;
        }

       // [Route("Index")]
        // GET: Service
        public async Task<IActionResult> Index(string keyword)
        {
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var serviceList = await _context.Services.Include(s => s.Clinic).Include(s => s.Specialty).Where(d => d.ClinicID == clinicId).ToListAsync();
            if (!string.IsNullOrEmpty(keyword))
            {
				keyword = keyword.Trim().ToLower();
				keyword = Util.ConvertVnString(keyword);
				serviceList = serviceList.Where(p =>
                    (Util.ConvertVnString(p.Specialty.Name).Contains(keyword)) ||
                    Util.ConvertVnString(p.Name).Contains(keyword) ||
                    Util.ConvertVnString(p.Price).Contains(keyword)).ToList();
            }
            return View(serviceList);
        }

		// GET: Service/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var service = await _context.Services
                .Include(s => s.Clinic)
                .Include(s => s.Specialty)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (service == null || service.ClinicID != clinicId)
            {
                return NotFound();
            }

            return View(service);
        }

		//[Route("Create")]
		// GET: Service/Create
		public IActionResult Create()
        {
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            ViewBag.ClinicID = clinicId;
            ViewData["SpecialtyID"] = new SelectList(_context.Specialties, "ID", "Name");
            return View();
        }

        // POST: Service/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ClinicID,SpecialtyID,Name,Description,Price")] ServiceVM service)
        {
            if (ModelState.IsValid)
            {
                var newSer = new Service
                {
                    ClinicID = service.ClinicID,
                    SpecialtyID = service.SpecialtyID,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price
                };
                _context.Add(newSer);
                await _context.SaveChangesAsync();
				TempData["ToastMessageSuccessTempData"] = "Thành công thêm mới dịch vụ.";
				return RedirectToAction(nameof(Index));
            }
            ViewBag.ClinicID = service.ClinicID;
            ViewData["SpecialtyID"] = new SelectList(_context.Specialties, "ID", "Name", service.SpecialtyID);
            return View(service);
        }

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var service = await _context.Services.FindAsync(id);
            if (service == null || service.ClinicID != clinicId)
            {
                return NotFound();
            }
            ViewBag.ClinicID = clinicId;
            ViewData["SpecialtyID"] = new SelectList(_context.Specialties, "ID", "Name", service.SpecialtyID);

            var serviceVM = new ServiceVM()
            {
                ID = service.ID,
                ClinicID = service.ClinicID,
                SpecialtyID = service.SpecialtyID,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price
            };
            return View(serviceVM);
        }

        // POST: Service/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ClinicID,SpecialtyID,Name,Description,Price")] ServiceVM service)
        {
            if (id != service.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var upSer = await _context.Services.FindAsync(id);
                    if (upSer != null)
                    {
                        upSer.SpecialtyID = service.SpecialtyID;
                        upSer.Name = service.Name;
                        upSer.Description = service.Description;
                        upSer.Price = service.Price;
                    }
                    _context.Update(upSer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
				TempData["ToastMessageSuccessTempData"] = "Thành công chỉnh sửa dịch vụ.";
				return RedirectToAction(nameof(Details), new { id = id });
            }
            ViewBag.ClinicID = service.ClinicID;
            ViewData["SpecialtyID"] = new SelectList(_context.Specialties, "ID", "Name", service.SpecialtyID);
			return View(service);
        }

        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var clinicId = HttpContext.Session.GetInt32("clinicId");
            if (clinicId == null)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var service = await _context.Services
                .Include(s => s.Clinic)
                .Include(s => s.Specialty)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (service == null || service.ClinicID != clinicId)
            {
                return NotFound();
            }
			return View(service);
        }

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
			TempData["ToastMessageSuccessTempData"] = "Thành công xóa dịch vụ.";
			return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ID == id);
        }
    }
}
