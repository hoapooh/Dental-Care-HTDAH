using Dental_Clinic_System.Areas.Admin.ViewModels;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.EmailSender;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin, Mini Admin")]
    public class ManagerClinicController : Controller
    {
        private readonly DentalClinicDbContext _context;
        private readonly IEmailSenderCustom _emailSender;

        public ManagerClinicController(DentalClinicDbContext context, IEmailSenderCustom emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        #region Show List Clinic
        //===================LIST CLINIC===================
        //[Route("ListClinic")]
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
									 ProvinceName = clinic.ProvinceName,
									 Image = clinic.Image,
									 Id = clinic.ID,
									 ManagerName = account.LastName + " " + account.FirstName
								 }).ToListAsync();

            return View(clinics);
        }
        #endregion

        #region Tìm kiếm phòng khám (Search)
        //===================TÌM KIẾM===================
        //[Route("SearchClinic")]
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

        #region Thêm phòng khám (ADD)
        //===================THÊM PHÒNG KHÁM===================
        [HttpGet]
        public async Task<IActionResult> CreateClinic()
        {
            var unassignedManagers = await _context.Accounts
            .Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
            .Select(a => new
            {
                a.ID,
                FullName = a.LastName + " " + a.FirstName
            }).ToListAsync();

            var model = new AddClincVM
            {
                UnassignedManagers = new SelectList(unassignedManagers, "ID", "FullName"),
            };

			var amTimes = new List<TimeOnly> {
				new TimeOnly(7, 0), new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
				new TimeOnly(9, 0), new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
				new TimeOnly(11, 0), new TimeOnly(11, 30), new TimeOnly(12, 0)
			};

			var pmTimes = new List<TimeOnly> {
				new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0), new TimeOnly(13, 30),
				new TimeOnly(14, 0), new TimeOnly(14, 30), new TimeOnly(15, 0), new TimeOnly(15, 30),
				new TimeOnly(16, 0), new TimeOnly(16, 30), new TimeOnly(17, 0), new TimeOnly(17, 30),
				new TimeOnly(18, 0), new TimeOnly(18, 30), new TimeOnly(19, 0), new TimeOnly(19, 30),
				new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
			};

			ViewBag.AmTimes = amTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.AmStartTime || t.ToString("HH:mm") == model.AmEndTime
			}).ToList();

			ViewBag.PmTimes = pmTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.PmStartTime || t.ToString("HH:mm") == model.PmEndTime
			}).ToList();

			return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> CreateClinic(AddClincVM model)
        {
            if (ModelState.IsValid)
            {
                //Kiểm tra Phòng khám đã tồn tại chưa
                bool clinicNameExists = await _context.Clinics.AnyAsync(c => c.Name == model.Name);
                if (clinicNameExists)
                {
                    ModelState.AddModelError("Name", "Tên phòng khám đã tồn tại.");
                }

                //Kiểm tra Hotline đã tồn tại chưa
                bool clinicPhoneExists = await _context.Clinics.AnyAsync(c => c.PhoneNumber == model.PhoneNumber);
                if (clinicPhoneExists)
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại đã tồn tại.");
                }

                //Kiểm tra Email phòng khám tồn tại chưa
                bool clinicEmailExists = await _context.Clinics.AnyAsync(c => c.Email == model.Email);
                if (clinicEmailExists)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                }

                //Nếu đã tồn tại, load lại danh sách
                if (!ModelState.IsValid)
                {
                    //List những quản lý chưa có phòng khám nào
                    var unassignedManager = await _context.Accounts
                        .Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
                        .Select(a => new
                        {
                            a.ID,
                            FullName = a.LastName + " " + a.FirstName
                        })
                        .ToListAsync();

                    model.UnassignedManagers = new SelectList(unassignedManager, "ID", "FullName");

                    //Thời gian làm việc buổi sáng + chiều
					var amTime = new List<TimeOnly> {
				        new TimeOnly(7, 0), new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
				        new TimeOnly(9, 0), new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
				        new TimeOnly(11, 0), new TimeOnly(11, 30), new TimeOnly(12, 0)
			        };

					var pmTime = new List<TimeOnly> {
				        new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0), new TimeOnly(13, 30),
				        new TimeOnly(14, 0), new TimeOnly(14, 30), new TimeOnly(15, 0), new TimeOnly(15, 30),
				        new TimeOnly(16, 0), new TimeOnly(16, 30), new TimeOnly(17, 0), new TimeOnly(17, 30),
				        new TimeOnly(18, 0), new TimeOnly(18, 30), new TimeOnly(19, 0), new TimeOnly(19, 30),
				        new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
			        };

					ViewBag.AmTimes = amTime.Select(t => new SelectListItem
					{
						Value = t.ToString("HH:mm"),
						Text = t.ToString("HH:mm"),
						Selected = t.ToString("HH:mm") == model.AmStartTime || t.ToString("HH:mm") == model.AmEndTime
					}).ToList();

					ViewBag.PmTimes = pmTime.Select(t => new SelectListItem
					{
						Value = t.ToString("HH:mm"),
						Text = t.ToString("HH:mm"),
						Selected = t.ToString("HH:mm") == model.PmStartTime || t.ToString("HH:mm") == model.PmEndTime
					}).ToList();

					return View(model);
                }


                //Kiểm tra Account Quản lý có tồn tại không, và đúng Role Quản lý chưa
                var manager = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ID == model.ManagerID && a.Role == "Quản Lý");

                if (manager == null)
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy Role là Quản Lý.");
                    return View(model);
                }

				var clinic = new Clinic
				{
					Name = model.Name,
					PhoneNumber = model.PhoneNumber,
					Email = model.Email,
					ManagerID = manager.ID,
					Province = model.Province,
					District = model.District,
					Ward = model.Ward,
					ProvinceName = model.ProvinceName,
					DistrictName = model.DistrictName,
					WardName = model.WardName,
					Address = model.Address,
					Description = model.Description,
					Image = model.Image,
					ClinicStatus = "Hoạt Động"
				};

				//Check thời gian làm việc có tồn tại không?
				//Nếu không, thêm vào
				TimeOnly amStart = TimeOnly.Parse(model.AmStartTime);
				TimeOnly amEnd = TimeOnly.Parse(model.AmEndTime);
				TimeOnly pmStart = TimeOnly.Parse(model.PmStartTime);
				TimeOnly pmEnd = TimeOnly.Parse(model.PmEndTime);

				var amWorkTime = await _context.WorkTimes
		            .FirstOrDefaultAsync(w => w.Session == "Sáng" && w.StartTime == amStart && w.EndTime == amEnd);
				if (amWorkTime == null)
				{
					amWorkTime = new WorkTime { Session = "Sáng", StartTime = amStart, EndTime = amEnd };
					_context.WorkTimes.Add(amWorkTime);
					await _context.SaveChangesAsync();
				}

				var pmWorkTime = await _context.WorkTimes
					.FirstOrDefaultAsync(w => w.Session == "Chiều" && w.StartTime == pmStart && w.EndTime == pmEnd);
				if (pmWorkTime == null)
				{
					pmWorkTime = new WorkTime { Session = "Chiều", StartTime = pmStart, EndTime = pmEnd };
					_context.WorkTimes.Add(pmWorkTime);
					await _context.SaveChangesAsync();
				}

				clinic.AmWorkTimeID = amWorkTime.ID;
				clinic.PmWorkTimeID = pmWorkTime.ID;

				_context.Clinics.Add(clinic);
                await _context.SaveChangesAsync();

                TempData["ToastMessageSuccessTempData"] = "Thêm mới phòng khám thành công";
                return RedirectToAction(nameof(ListClinic));
			}

			// Load lại dữ liệu khi ModelState không hợp lệ
			var unassignedManagers = await _context.Accounts
				.Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
				.Select(a => new
				{
					a.ID,
					FullName = a.LastName + " " + a.FirstName
				}).ToListAsync();

            model.UnassignedManagers = new SelectList(unassignedManagers, "ID", "FullName");


			var amTimes = new List<TimeOnly> {
				new TimeOnly(7, 0), new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
				new TimeOnly(9, 0), new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
				new TimeOnly(11, 0), new TimeOnly(11, 30), new TimeOnly(12, 0)
			};

			var pmTimes = new List<TimeOnly> {
				new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0), new TimeOnly(13, 30),
				new TimeOnly(14, 0), new TimeOnly(14, 30), new TimeOnly(15, 0), new TimeOnly(15, 30),
				new TimeOnly(16, 0), new TimeOnly(16, 30), new TimeOnly(17, 0), new TimeOnly(17, 30),
				new TimeOnly(18, 0), new TimeOnly(18, 30), new TimeOnly(19, 0), new TimeOnly(19, 30),
				new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
			};

			ViewBag.AmTimes = amTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.AmStartTime || t.ToString("HH:mm") == model.AmEndTime
			}).ToList();

			ViewBag.PmTimes = pmTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.PmStartTime || t.ToString("HH:mm") == model.PmEndTime
			}).ToList();

			return View(model);
        }
        #endregion

        #region Chỉnh sửa (Edit Clinic)
        //===================CHỈNH SỬA PHÒNG KHÁM===================
        [HttpGet]
        public async Task<IActionResult> EditClinic(int id)
        {
			var clinic = await _context.Clinics
		        .Include(c => c.AmWorkTimes)
		        .Include(c => c.PmWorkTimes)
		        .FirstOrDefaultAsync(c => c.ID == id);

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
				ProvinceName = clinic.ProvinceName,
				DistrictName = clinic.DistrictName,
				WardName = clinic.WardName,
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
					}).ToListAsync(), "ID", "FullName"),

				AmStartTime = clinic.AmWorkTimes?.StartTime.ToString("HH:mm") ?? "07:00",
				AmEndTime = clinic.AmWorkTimes?.EndTime.ToString("HH:mm") ?? "11:00",
				PmStartTime = clinic.PmWorkTimes?.StartTime.ToString("HH:mm") ?? "13:00",
				PmEndTime = clinic.PmWorkTimes?.EndTime.ToString("HH:mm") ?? "21:00"
			};

			var amTimes = new List<TimeOnly> {
				new TimeOnly(7, 0), new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
				new TimeOnly(9, 0), new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
				new TimeOnly(11, 0), new TimeOnly(11, 30), new TimeOnly(12, 0)
			};

			var pmTimes = new List<TimeOnly> {
				new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0), new TimeOnly(13, 30),
				new TimeOnly(14, 0), new TimeOnly(14, 30), new TimeOnly(15, 0), new TimeOnly(15, 30),
				new TimeOnly(16, 0), new TimeOnly(16, 30), new TimeOnly(17, 0), new TimeOnly(17, 30),
				new TimeOnly(18, 0), new TimeOnly(18, 30), new TimeOnly(19, 0), new TimeOnly(19, 30),
				new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
			};

			ViewBag.AmTimes = amTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.AmStartTime || t.ToString("HH:mm") == model.AmEndTime
			}).ToList();

			ViewBag.PmTimes = pmTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.PmStartTime || t.ToString("HH:mm") == model.PmEndTime
			}).ToList();

			return View(model);
        }

		[HttpPost]
		public async Task<IActionResult> EditClinic(AddClincVM model)
		{
			if (ModelState.IsValid)
			{
				var clinic = await _context.Clinics
			        .Include(c => c.AmWorkTimes)
			        .Include(c => c.PmWorkTimes)
			        .FirstOrDefaultAsync(c => c.ID == model.ID);

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
				clinic.ProvinceName = model.ProvinceName;
				clinic.DistrictName = model.DistrictName;
				clinic.WardName = model.WardName;
				clinic.Address = model.Address;
				clinic.Description = model.Description;
				clinic.Image = model.Image;
				clinic.ClinicStatus = "Hoạt Động";

				//Xử lý thời gian làm việc
				TimeOnly amS = TimeOnly.Parse(model.AmStartTime);
				TimeOnly amE = TimeOnly.Parse(model.AmEndTime);
				TimeOnly pmS = TimeOnly.Parse(model.PmStartTime);
				TimeOnly pmE = TimeOnly.Parse(model.PmEndTime);

				var amWorkTime = await _context.WorkTimes.FirstOrDefaultAsync(w => w.Session == "Sáng" && w.StartTime == amS && w.EndTime == amE);
				if (amWorkTime == null)
				{
					amWorkTime = new WorkTime { Session = "Sáng", StartTime = amS, EndTime = amE };
					_context.WorkTimes.Add(amWorkTime);
					await _context.SaveChangesAsync();
				}

				var pmWorkTime = await _context.WorkTimes.FirstOrDefaultAsync(w => w.Session == "Chiều" && w.StartTime == pmS && w.EndTime == pmE);
				if (pmWorkTime == null)
				{
					pmWorkTime = new WorkTime { Session = "Chiều", StartTime = pmS, EndTime = pmE };
					_context.WorkTimes.Add(pmWorkTime);
					await _context.SaveChangesAsync();
				}

				clinic.AmWorkTimeID = amWorkTime.ID;
				clinic.PmWorkTimeID = pmWorkTime.ID;

				_context.Update(clinic);
				await _context.SaveChangesAsync();

                TempData["ToastMessageSuccessTempData"] = "Chỉnh sửa phòng khám thành công";
                return RedirectToAction(nameof(ListClinic));
			}

			//Load lại dữ liệu khi ModelState không hợp lệ
			//Ghi lại List Manager chưa được chỉ định phòng khám nào
			model.UnassignedManagers = new SelectList(await _context.Accounts
                .Where(a => a.Role == "Quản Lý" && !_context.Clinics.Any(c => c.ManagerID == a.ID))
                .Select(a => new
                {
                    a.ID,
                    FullName = a.LastName + " " + a.FirstName
                }).ToListAsync(), "ID", "FullName");

			var amTimes = new List<TimeOnly> {
				new TimeOnly(7, 0), new TimeOnly(7, 30), new TimeOnly(8, 0), new TimeOnly(8, 30),
				new TimeOnly(9, 0), new TimeOnly(9, 30), new TimeOnly(10, 0), new TimeOnly(10, 30),
				new TimeOnly(11, 0), new TimeOnly(11, 30), new TimeOnly(12, 0)
			};

			var pmTimes = new List<TimeOnly> {
				new TimeOnly(12, 0), new TimeOnly(12, 30), new TimeOnly(13, 0), new TimeOnly(13, 30),
				new TimeOnly(14, 0), new TimeOnly(14, 30), new TimeOnly(15, 0), new TimeOnly(15, 30),
				new TimeOnly(16, 0), new TimeOnly(16, 30), new TimeOnly(17, 0), new TimeOnly(17, 30),
				new TimeOnly(18, 0), new TimeOnly(18, 30), new TimeOnly(19, 0), new TimeOnly(19, 30),
				new TimeOnly(20, 0), new TimeOnly(20, 30), new TimeOnly(21, 0)
			};

			ViewBag.AmTimes = amTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.AmStartTime || t.ToString("HH:mm") == model.AmEndTime
			}).ToList();

			ViewBag.PmTimes = pmTimes.Select(t => new SelectListItem
			{
				Value = t.ToString("HH:mm"),
				Text = t.ToString("HH:mm"),
				Selected = t.ToString("HH:mm") == model.PmStartTime || t.ToString("HH:mm") == model.PmEndTime
			}).ToList();

			return View(model);
        }
        #endregion

        #region Đóng cửa phòng khám (Delete)
        //===================XÓA PHÒNG KHÁM===================
        //[Route("HiddenClinic")]
        public async Task<IActionResult> HiddenClinic(string name, string status)
        {
            var clinic = await _context.Clinics.SingleOrDefaultAsync(c => c.Name == name);

            if (clinic != null)
            {
                clinic.ClinicStatus = status;
                _context.SaveChanges();
            }

            TempData["ToastMessageSuccessTempData"] = "Đóng cửa phòng khám thành công";
            return RedirectToAction(nameof(ListClinic));
		}
		#endregion

        //=====================PHÒNG KHÁM ĐÓNG CỬA=====================

        #region Show Clinic Closed
        //[Route("ListClinicClosed")]
        public async Task<IActionResult> ListClinicClosed()
        {
            var clinicClosed = await (from clinic in _context.Clinics
                                      join account in _context.Accounts
                                      on clinic.ManagerID equals account.ID

									  where account.Role == "Quản Lý" && clinic.ClinicStatus == "Đóng Cửa"
									  select new ManagerClinicVM
									  {
										  ClinicName = clinic.Name,
										  ProvinceName = clinic.ProvinceName,
										  Id = clinic.ID,
										  Image = clinic.Image,
										  ManagerName = account.LastName + " " + account.FirstName
									  }).ToListAsync();

            return View(clinicClosed);

        }
        #endregion

        #region Xem phòng khám (View)
        [HttpGet]
        //[Route("ViewClinic/{id}")]
        public async Task<IActionResult> ViewClinic(int id)
        {
            var clinic = await (from c in _context.Clinics
                                join a in _context.Accounts
                                on c.ManagerID equals a.ID
                                where c.ID == id && a.Role == "Quản Lý" && c.ClinicStatus == "Đóng Cửa"
                                select new ManagerClinicVM
                                {
                                    Id = c.ID,
                                    Name = c.Name,
                                    PhoneNumber = c.PhoneNumber,
                                    Email = c.Email,
                                    ManagerName = a.LastName + " " + a.FirstName,
                                    ProvinceName = c.ProvinceName,
                                    DistrictName = c.DistrictName,
                                    WardName = c.WardName,
                                    Address = c.Address,
                                    Description = c.Description,
                                    Image = c.Image,
                                    ClinicStatus = c.ClinicStatus
                                }).FirstOrDefaultAsync();

            if (clinic == null)
            {
                return NotFound();
            }

            return Json(clinic);
        }
        #endregion

        #region Mở cửa lại phòng khám
        //[Route("UnlockClinic")]
        public async Task<IActionResult> UnlockClinic(string name, string status)
        {
            var clinic = await _context.Clinics.SingleOrDefaultAsync(c => c.Name == name);

            if (clinic != null)
            {
                clinic.ClinicStatus = status;
                _context.SaveChanges();
            }

            TempData["ToastMessageSuccessTempData"] = "Mở cửa phòng khám thành công";
            return RedirectToAction(nameof(ListClinicClosed));
		}
		#endregion

        #region Duyệt Yêu Cầu Hợp Tác Kinh Doanh
        [HttpGet]
        //[Route("ApprovalRequest")]
        public async Task<IActionResult> ApprovalRequest()
        {
            var orderList = await _context.Orders.ToListAsync();
            var businessPartnershipViewModel = new BusinessPartnershipViewModel
            {
                BusinessPartnerships = orderList
            };

            return View(businessPartnershipViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ViewApprovalRequest(int id)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(o => o.ID == id);
            if(order == null)
            {
                return NotFound();
            }

            var amWorkTime = await _context.WorkTimes.SingleOrDefaultAsync(w => w.ID == order.AmWorkTimeID);
            var pmWorkTime = await _context.WorkTimes.SingleOrDefaultAsync(w => w.ID == order.PmWorkTimeID);

            var orderVM = new OrderVM
            {
                ID = id,
                CompanyName = order.CompanyName,
                CompanyPhonenumber = order.CompanyPhonenumber,
                CompanyEmail = order.CompanyEmail,
                ManagerPhonenumber = order.ManagerPhonenumber,
                ManagerEmail = order.ManagerEmail,
                RepresentativeName = order.RepresentativeName,
                ClinicName = order.ClinicName,
                DomainName = order.DomainName,
                Content = order.Content,
                Image = order.Image,
                Status = order.Status,

                AmWorkTime = $"{amWorkTime?.Session}: {amWorkTime?.StartTime.ToString("HH:mm")} - {amWorkTime?.EndTime.ToString("HH:mm")}",
                PmWorkTime = $"{pmWorkTime?.Session}: {pmWorkTime?.StartTime.ToString("HH:mm")} - {pmWorkTime?.EndTime.ToString("HH:mm")}",

                ClinicAddress = $"{order.ClinicAddress}, {await LocalAPIReverseString.GetProvinceNameById(order.Province ?? 0)}, {await LocalAPIReverseString.GetDistrictNameById(order.Province ?? 0, order.District ?? 0)}, {await LocalAPIReverseString.GetWardNameById(order.District ?? 0, order.Ward ?? 0)}"
            };

            return Json(orderVM);
        }

        [HttpPost]
        //[Route("GetApprovalRequest")]
        public async Task<IActionResult> GetApprovalRequest(string companyName, string companyPhonenumber, string companyEmail, string representativeName, string clinicName, string clinicAddress, string? domainName, string managerPhonenumber, string managerEmail, string logo, int provinceID, int districtID, int wardID, string amStartTime, string amEndTime, string pmStartTime, string pmEndTime, string content)
        {

            #region Parse String To TimeOnly
            TimeOnly amS = TimeOnly.Parse(amStartTime);
            TimeOnly amE = TimeOnly.Parse(amEndTime);
            TimeOnly pmS = TimeOnly.Parse(pmStartTime);
            TimeOnly pmE = TimeOnly.Parse(pmEndTime);
            #endregion

            if (amS >= amE || pmS >= pmE)
            {
                TempData["ToastMessageFailTempData"] = "Thời gian làm việc không phù hợp";
                return RedirectToAction("index", "contact", new { area = "" });
            }

            var checkResult = await CheckExistingDetails(companyName, companyPhonenumber, companyEmail, managerEmail, managerPhonenumber, clinicName, domainName);

            switch (checkResult)
            {
                case "CompanyNameExists":
                    TempData["ToastMessageFailTempData"] = "Tên doanh nghiệp đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });

                case "CompanyPhonenumberExists":
                    TempData["ToastMessageFailTempData"] = "Hotline đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });

                case "CompanyEmailExists":
                    TempData["ToastMessageFailTempData"] = "Địa chỉ Email Doanh Nghiệp đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });

                case "ManagerPhonenumberExists":
                    TempData["ToastMessageFailTempData"] = "Số điện thoại liên hệ đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });

                case "ManagerEmailExists":
                    TempData["ToastMessageFailTempData"] = "Địa chỉ Email Người Đại Diện đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });

                case "ClinicNameExists":
                    TempData["ToastMessageFailTempData"] = "Tên phòng khám đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });

                case "DomainNameExists":
                    TempData["ToastMessageFailTempData"] = "Tên miền đã được đăng ký";
                    return RedirectToAction("index", "contact", new { area = "" });
            }

            #region Check worktime is alredy existed or yet
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
            var amWorkTime = await _context.WorkTimes.FirstOrDefaultAsync(t => t.Session == "Sáng" && t.StartTime == amS && t.EndTime == amE);
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
            var pmWorkTime = await _context.WorkTimes.FirstOrDefaultAsync(t => t.Session == "Chiều" && t.StartTime == pmS && t.EndTime == pmE);
            #endregion

            var order = new Order
            {
                CompanyName = companyName,
                CompanyPhonenumber = companyPhonenumber,
                CompanyEmail = companyEmail,
                ManagerPhonenumber = managerPhonenumber,
                ManagerEmail = managerEmail,
                RepresentativeName = representativeName,
                ClinicName = clinicName,
                ClinicAddress = clinicAddress,
                DomainName = domainName,
                Content = content,
                Image = logo,
                Province = provinceID,
                District = districtID,
                Ward = wardID,
                AmWorkTimeID = amWorkTime.ID,
                PmWorkTimeID =  pmWorkTime.ID,
                Status = "Chưa Duyệt"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            TempData["ToastMessageSuccessTempData"] = "Gửi thông tin thành công";
            return RedirectToAction("index", "contact", new { area = "" });
        }

        private async Task<string> CheckExistingDetails(string companyName, string companyPhonenumber, string companyEmail, string managerEmail, string managerPhonenumber, string clinicName, string domainName)
        {
            if (await _context.Orders.AnyAsync(c => c.CompanyName == companyName))
            {
                return "CompanyNameExists";
            }

            if (await _context.Orders.AnyAsync(c => c.CompanyPhonenumber == companyPhonenumber))
            {
                return "CompanyPhonenumberExists";
            }

            if (await _context.Orders.AnyAsync(c => c.CompanyEmail == companyEmail))
            {
                return "CompanyEmailExists";
            }

            if(await _context.Orders.AnyAsync(c => c.ManagerPhonenumber == managerPhonenumber))
            {
                return "ManagerPhonenumberExists";
            }

            if( await _context.Orders.AnyAsync(c => c.ManagerEmail == managerEmail))
            {
                return "ManagerEmailExists";
            }

            if (await _context.Orders.AnyAsync(c => c.ClinicName == clinicName))
            {
                return "ClinicNameExists";
            }

            if (await _context.Orders.AnyAsync(c => c.DomainName == domainName))
            {
                return "DomainNameExists";
            }

            return "None";
        }

        [HttpPost]
        //[Route("ProcessRequest")]
        public async Task<IActionResult> ProcessRequest(int orderID, string orderStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.ID == orderID);
            if (order == null)
            {
                return NotFound();
            }

            if (orderStatus == "Đồng Ý")
            {
                string[] nameParts = order.RepresentativeName.Trim().Split(' ');

                var encryptedPassword = Util.GenerateRandomKey(order.CompanyEmail, 20);
                var user = new Account
                {
                    Username = order.CompanyEmail.Trim(),
                    Password = DataEncryptionExtensions.ToMd5Hash(encryptedPassword),
                    Role = "Quản Lý",
                    FirstName = string.Join(" ", nameParts.Take(nameParts.Length - 1)),
                    LastName = nameParts.Last(),
                    Email = order.ManagerEmail,
                    PhoneNumber = order.ManagerPhonenumber,
                    AccountStatus = "Hoạt Động"
                };

                order.Status = "Đồng Ý";

                _context.Accounts.Add(user);
                await _context.SaveChangesAsync();

                var managerBalance = new Wallet
                {
                    Account_ID = user.ID,
                    Money = 0
                };

                var clinic = new Clinic
                {
                    Name = order.ClinicName,
                    PhoneNumber = order.CompanyPhonenumber,
                    Email = order.CompanyEmail,
                    ManagerID = user.ID,

                    Province = order.Province,
                    ProvinceName = await LocalAPIReverseString.GetProvinceNameById(order.Province ?? 0),
                    District = order.District,
                    DistrictName = await LocalAPIReverseString.GetDistrictNameById(order.Province ?? 0, order.District ?? 0),
                    Ward = order.Ward,
                    WardName = await LocalAPIReverseString.GetWardNameById(order.District ?? 0, order.Ward ?? 0),

                    Address = order.ClinicAddress,
                    Description = null,
                    Image = order.Image,
                    ClinicStatus = "Hoạt Động",
                    AmWorkTimeID = order.AmWorkTimeID,
                    PmWorkTimeID = order.PmWorkTimeID
                };
                _context.Add(clinic);
                await _context.SaveChangesAsync();

                await _emailSender.SendBusinessPartnershipsInfo(order, user, encryptedPassword, "Xác nhận trở thành đối tác của Dental Care");

                TempData["ToastMessageSuccessTempData"] = "Xác nhận đơn duyệt thành công";
                return RedirectToAction("ApprovalRequest", "ManagerClinic", new { area = "admin" });
            }
            else if (orderStatus == "Từ Chối")
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                TempData["ToastMessageSuccessTempData"] = "Từ chối đơn duyệt thành công";
                return RedirectToAction("ApprovalRequest", "ManagerClinic", new { area = "admin" });
            }

            TempData["ToastMessageTempData"] = "Đã có lỗi xảy ra";
            return RedirectToAction("ApprovalRequest", "ManagerClinic", new { area = "admin" });
        }
        #endregion

    }
}