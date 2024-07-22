using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Areas.Manager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using StackExchange.Redis;
using Dental_Clinic_System.Areas.Admin.ViewModels;
using NuGet.Protocol;
using NuGet.Common;

namespace Dental_Clinic_System.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(AuthenticationSchemes = "ManagerScheme", Roles = "Quản Lý")]
    public class ClinicsController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public ClinicsController(DentalClinicDbContext context)
        {
            _context = context;
        }

        // GET: Manager/Clinics/Edit/5
        public async Task<IActionResult> Edit()
        {
            int clinicId = HttpContext.Session.GetInt32("clinicId") ?? 0;
            if (clinicId == 0)
            {   // Check if session has expired, log out
                return RedirectToAction("Logout", "ManagerAccount", new { area = "Manager" });
            }
            var clinic = await _context.Clinics.Include(c => c.AmWorkTimes).Include(c => c.PmWorkTimes).AsQueryable().FirstOrDefaultAsync(s => s.ID == clinicId);
            if (clinic == null || clinic.ID != clinicId)
            {
                return NotFound();
            }
            //-----------------------------------------------
            var clinicVM = new ClinicVM
            { 
                ID = clinicId,
                ManagerID = clinic.ManagerID,
                Name = clinic.Name,
                Province = clinic.Province,
                Ward = clinic.Ward,
                District = clinic.District,
                Address = clinic.Address,
                PhoneNumber = clinic.PhoneNumber,
                Email = clinic.Email,
                Description = clinic.Description,
                Image = clinic.Image,
                ClinicStatus = clinic.ClinicStatus,
                MapLinker = clinic.MapLinker,
                AmStartTime = clinic.AmWorkTimes.StartTime,
                AmEndTime = clinic.AmWorkTimes.EndTime,
                PmStartTime = clinic.PmWorkTimes.StartTime,
                PmEndTime = clinic.PmWorkTimes.EndTime
            };

            ViewBag.AmTimes = new List<TimeOnly>() {  // 7:00, 7:30, 8:00...
                new TimeOnly(7, 0),  new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
                new TimeOnly(9, 0),  new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
                new TimeOnly(11, 0),  new TimeOnly(11, 30), new TimeOnly(12, 0)
            };
            ViewBag.PmTimes = new List<TimeOnly>() {  // 7:00, 7:30, 8:00...
                new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0),  new TimeOnly(13, 30), 
                new TimeOnly(14, 0), new TimeOnly(14, 30),
                new TimeOnly(15, 0),  new TimeOnly(15, 30), new TimeOnly(16, 0), new TimeOnly(16, 30),
                new TimeOnly(17, 0),  new TimeOnly(17, 30), new TimeOnly(18, 0), new TimeOnly(18, 30),
                new TimeOnly(19, 0),  new TimeOnly(19, 30), new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
            };
            
            return View(clinicVM);
        }

        // POST: Manager/Clinics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("ID,ManagerID,Name,Province,Ward,District,Address,PhoneNumber,Email,Description,MapLinker,Image,ClinicStatus,AmStartTime,AmEndTime,PmStartTime,PmEndTime")] ClinicVM clinic)
        {
            //--------------------------------------
            ViewBag.AmTimes = new List<TimeOnly>() {  // 7:00, 7:30, 8:00...
                new TimeOnly(7, 0),  new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
                new TimeOnly(9, 0),  new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
                new TimeOnly(11, 0),  new TimeOnly(11, 30), new TimeOnly(12, 0)
            };
            ViewBag.PmTimes = new List<TimeOnly>() {  // 7:00, 7:30, 8:00...
                new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0),  new TimeOnly(13, 30),
                new TimeOnly(14, 0), new TimeOnly(14, 30),
                new TimeOnly(15, 0),  new TimeOnly(15, 30), new TimeOnly(16, 0), new TimeOnly(16, 30),
                new TimeOnly(17, 0),  new TimeOnly(17, 30), new TimeOnly(18, 0), new TimeOnly(18, 30),
                new TimeOnly(19, 0),  new TimeOnly(19, 30), new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
            };
            //-----------------------------------------
            if (ModelState.IsValid)
            {
                try
                {
                    var upClinic = await _context.Clinics.FindAsync(clinic.ID);
                    if (upClinic != null)
                    {
                        TimeOnly amS = clinic.AmStartTime;
                        TimeOnly amE = clinic.AmEndTime;
                        TimeOnly pmS = clinic.PmStartTime;
                        TimeOnly pmE = clinic.PmEndTime;
                        //--------------------------------------KIỂM TRA GIỜ HỢP LỆ KHÔNG?
                        if (amS > amE || pmS > pmE)
                        {
                            TempData["ToastMessageFailTempData"] = "Thời gian làm việc không hợp lệ.";
                            return View(clinic);
                        }
                        //--------------
                        var checkExistAm = await _context.WorkTimes.AnyAsync(t => t.Session == "Sáng" && t.StartTime == amS && t.EndTime == amE);
                        if (checkExistAm == false)
                        {
                            _context.WorkTimes.Add(new WorkTime
                            {
                                Session = "Sáng",
                                StartTime = amS,
                                EndTime = amE
                            });
                            await _context.SaveChangesAsync();
                        }
                        var am = await _context.WorkTimes.FirstOrDefaultAsync(t => t.Session == "Sáng" && t.StartTime == amS && t.EndTime == amE);
                        //--------------
                        var checkExistPm = await _context.WorkTimes.AnyAsync(t => t.Session == "Chiều" && t.StartTime == pmS && t.EndTime == pmE);
                        if (checkExistPm == false)
                        {
                            _context.WorkTimes.Add(new WorkTime
                            {
                                Session = "Chiều",
                                StartTime = pmS,
                                EndTime = pmE
                            });
                            await _context.SaveChangesAsync();
                        }
                        var pm = await _context.WorkTimes.FirstOrDefaultAsync(t => t.Session == "Chiều" && t.StartTime == pmS && t.EndTime == pmE);
                        //------------------------------------------------------
                        upClinic.Name = clinic.Name;
                        upClinic.AmWorkTimeID = am.ID;
                        upClinic.PmWorkTimeID = pm.ID;
                        upClinic.Image = clinic.Image ?? "";
                        upClinic.Province = clinic.Province;
                        upClinic.Ward = clinic.Ward;
                        upClinic.District = clinic.District;
                        upClinic.Address = clinic.Address ?? "";
                        upClinic.Email = clinic.Email;
                        upClinic.PhoneNumber = clinic.PhoneNumber;
                        upClinic.Description = clinic.Description;
                        upClinic.MapLinker = clinic.MapLinker;
                        upClinic.ClinicStatus = clinic.ClinicStatus;
                        _context.Update(upClinic);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClinicExists(clinic.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                HttpContext.Session.SetString("image", clinic?.Image ?? "");
                TempData["ToastMessageSuccessTempData"] = "Chỉnh sửa thành công.";
                return RedirectToAction(nameof(Edit));
            }
            return View(clinic);
            //List<string> errors = new List<string>();
            //foreach (var value in ModelState.Values)
            //{
            //    foreach (var error in value.Errors)
            //    {
            //        errors.Add(error.ErrorMessage);
            //    }
            //}
            //string errorMessage = string.Join("\n", errors);
            //return BadRequest(errorMessage);
        }


        private bool ClinicExists(int id)
        {
            return _context.Clinics.Any(e => e.ID == id);
        }
    }
}
