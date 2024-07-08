using Microsoft.AspNetCore.Mvc;
using Dental_Clinic_System.Models.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Dental_Clinic_System.Controllers
{
    [Area("Manager")]
    public class WalletController : Controller
    {
        private readonly DentalClinicDbContext _context;

        public WalletController(DentalClinicDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> GetCurrentBalance(int accountID)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Account.ID == accountID);
            if (wallet != null)
            {
                return Json(new { success = true, balance = wallet.Money });
            }
            return Json(new { success = false, message = "Wallet not found" });
        }
    }
}
