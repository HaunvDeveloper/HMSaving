using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class DebtController : Controller
    {
        private readonly QlchiTieuContext _context;
        public DebtController(QlchiTieuContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DebtSummaryPartial()
        {
            var userId = GetCurrentUserId();

            var data = await _context.Partners
                .Select(p => new PartnerDebtViewModel
                {
                    PartnerId = p.PartnerId,
                    PartnerName = p.PartnerName,
                    NetDebtAmount =
                        // Tổng tiền nợ người khác (mình cho mượn)
                        p.Debts.Where(d => d.UserId == userId && !d.InDebt).Sum(d => d.Amount)
                        // Trừ đi số đã thu về
                        - p.PayDebts.Where(pd => pd.UserId == userId && !pd.InDebt).Sum(pd => pd.Amount)
                        // Trừ đi tiền người khác nợ mình (mình đi vay)
                        - p.Debts.Where(d => d.UserId == userId && d.InDebt).Sum(d => d.Amount)
                        // Cộng lại số tiền mình đã trả
                        + p.PayDebts.Where(pd => pd.UserId == userId && pd.InDebt).Sum(pd => pd.Amount)
                })
                .ToListAsync();

            return PartialView(data);
        }

        [HttpGet]
        public IActionResult CreateDebt()
        {
            var viewModel = new CreateDebtViewModel
            {
                DebtDate = DateTime.Today,
               
            };

            ViewBag.Partners = _context.Partners.ToList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDebt(CreateDebtViewModel model)
        {
            if (!ModelState.IsValid)
            {

                ViewBag.Partners = _context.Partners.ToList();

                return View(model);
            }

            var userId = GetCurrentUserId(); // Viết hàm lấy userId từ Claims

            var debt = new Debt
            {
                PartnerId = model.PartnerId,
                UserId = userId,
                InDebt = model.InDebt,
                Amount = model.Amount * 1000,
                DebtDate = model.DebtDate,
                CreatedDate = DateTime.Now,
                Description = model.Description
            };

            _context.Debts.Add(debt);
            _context.SaveChanges();

            return RedirectToAction("Index", "Debt"); // hoặc về trang công nợ
        }



        public IActionResult PayDebt(long partnerId)
        {
            var partner = _context.Partners
                .Include(p => p.Debts)
                .Include(p => p.PayDebts)
                .FirstOrDefault(p => p.PartnerId == partnerId);

            if (partner == null) return NotFound();

            var net = partner.Debts.Where(d => !d.InDebt).Sum(d => d.Amount)
                      - partner.PayDebts.Where(p => !p.InDebt).Sum(p => p.Amount)
                      - partner.Debts.Where(d => d.InDebt).Sum(d => d.Amount)
                      + partner.PayDebts.Where(p => p.InDebt).Sum(p => p.Amount);

            var vm = new PayDebtViewModel
            {
                PartnerId = partner.PartnerId,
                PartnerName = partner.PartnerName,
                NetDebtAmount = net
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayDebt(PayDebtViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payDebt = new PayDebt
            {
                PartnerId = model.PartnerId,
                UserId = GetCurrentUserId(), // bạn cần xử lý theo context
                Amount = model.Amount,
                Description = model.Description,
                PaymentDate = model.PaymentDate,
                InDebt = model.InDebt,
                CreatedAt = DateTime.Now
            };

            _context.PayDebts.Add(payDebt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Debt");
        }










        private long GetCurrentUserId()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return userId;
        }
    
    
    
    
    }
}
