using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Dental_Clinic_System.Controllers
{
    public class NewsController : Controller
    {
        private readonly DentalClinicDbContext _context;
        public NewsController(DentalClinicDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var newsList = await _context.News.ToListAsync();

            var newsViewModelList = _context.News.AsNoTracking()
                             .Select(news => new NewsVM
                             {
                                 ID = news.ID,
                                 AccountID = news.AccountID,
                                 CreatedDate = news.CreatedDate,
                                 Title = news.Title,
                                 Content = HtmlHelpers.ExtractTextAndLimitCharacters(news.Content, 100),
                                 ThumbNail = news.ThumbNail,
                                 Status = news.Status
                             });

            return View(newsViewModelList);
        }

        [HttpGet]
        public async Task<IActionResult> NewsDetail(int newsID)
        {
            await Console.Out.WriteLineAsync($"NewsID = {newsID}");
            var news = await _context.News.FirstOrDefaultAsync(c => c.ID == newsID);
            string? content = news?.Content;
            ViewBag.News = news;

            return View();
        }
    }
}
