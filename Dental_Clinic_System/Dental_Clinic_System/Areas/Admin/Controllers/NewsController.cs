using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Areas.Admin.Controllers
{
	[Area("Admin")]
	//[Authorize(AuthenticationSchemes = "GetAppointmentStatus", Roles = "Admin")]
	public class NewsController : Controller
	{
		private readonly DentalClinicDbContext _context;
		public NewsController(DentalClinicDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<IActionResult> NewsPost()
		{
			var news = _context.News.ToList();
			return View(news);
		}

		[HttpGet]
		public async Task<IActionResult> NewsPostAdd()
		{
			var user = _context.Accounts.FirstOrDefault(r => r.Role == "Admin");

			ViewBag.AdminID = user?.ID;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateNewsPost(string? content, string? newsTitle, int adminID, string? thumbnail)
		{
			

			var news = new News
			{
				AccountID = adminID,
				Title = newsTitle,
				Content = content,
				CreatedDate = Util.GetUtcPlus7Time(),
				ThumbNail = thumbnail,
				Status = null
			};

			_context.News.Add(news);
			await _context.SaveChangesAsync();
			return RedirectToAction("newspost", "news", new { area = "Admin" });
		}

		[HttpGet]
		public async Task<IActionResult> NewsPostEdit(int id)
		{
			var news = await _context.News.FindAsync(id);
			return View(news);
		}

		[HttpPost]
		public async Task<IActionResult> EditNewsPost(int newsID, string? content, string? newsTitle, string? newsThumbnail)
		{
			var news = await _context.News.FindAsync(newsID);
			news.Title = newsTitle;
			news.Content = content;

			if (newsThumbnail != null)
			{
				news.ThumbNail = newsThumbnail;
			}

            _context.News.Update(news);
			await _context.SaveChangesAsync();
			return RedirectToAction("newspost", "news", new { area = "Admin" });
		}

		[HttpPost]
		public async Task<IActionResult> NewsPostDelete(int id)
		{
			var news = await _context.News.FindAsync(id);

			if (news != null)
			{
				_context.News.Remove(news);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("Newspost", "News", new { area = "Admin" });
		}

		[HttpGet]
		public async Task<IActionResult> SearchNewsPost(string? search)
		{
			if (string.IsNullOrEmpty(search))
			{
				return RedirectToAction("newspost", "news", new { area = "Admin" });
			}
			var news = _context.News.Where(r => r.Title.Contains(search)).ToList();
			return View("NewsPost", news);
		}
	}
}
