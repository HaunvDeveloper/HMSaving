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
                .Where(p => p.UserId == userId)
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
                .OrderByDescending(p => Math.Abs(p.NetDebtAmount))
                .ToListAsync();

            return PartialView(data);
        }


        public IActionResult DebtHistory(long partnerId)
        {
            long userId = GetCurrentUserId();
            var partner = _context.Partners.FirstOrDefault(p => p.PartnerId == partnerId && p.UserId == userId);
            if (partner == null) return NotFound();
            return View(partner);
        }

        [HttpGet]
        public async Task<IActionResult> GetDebtHistory(long partnerId)
        {
            var debts = await _context.Debts
                .Where(d => d.PartnerId == partnerId && d.UserId == GetCurrentUserId())
                .Select(d => new DebtTransactionViewModel
                {
                    Id = d.DebtId,
                    Type = d.InDebt ? "Nợ" : "Cho nợ", // Nợ nếu là mình nợ, Cho nợ nếu là họ nợ mình
                    State = "Debt", // Trạng thái là "Debt" cho nợ
                    Amount = d.Amount, // Giả sử Amount lưu theo đơn vị nghìn đồng
                    TransactionDate = d.DebtDate,
                    CreatedDate = d.CreatedDate,
                    Description = d.Description,
                    InDebt = d.InDebt // true nếu là nợ, false nếu là họ nợ
                })
                .ToListAsync();
            var payDebts = await _context.PayDebts
                .Where(pd => pd.PartnerId == partnerId && pd.UserId == GetCurrentUserId())
                    .Select(pd => new DebtTransactionViewModel
                    {
                        Id = pd.PayDebtId,
                        Type = pd.InDebt ? "Trả nợ" : "Thu nợ",
                        State = "PayDebt", // Trạng thái là "PayDebt" cho trả nợ
                        Amount = pd.Amount, // Giả sử Amount lưu theo đơn vị nghìn đồng
                        TransactionDate = pd.PaymentDate,
                        CreatedDate = pd.CreatedAt,
                        Description = pd.Description,
                        InDebt = pd.InDebt // true nếu là nợ, false nếu là họ nợ
                    })
                .ToListAsync();
            var transactionList = debts.Concat(payDebts)
                .OrderByDescending(x => x.TransactionDate)
                    .ThenByDescending(x => x.CreatedDate)
                .ToList();

            return Json(new { data = transactionList });
        }

        public IActionResult CreateDebtTogether()
        {
            var partners = _context.Partners.Where(d => d.UserId == GetCurrentUserId()).ToList();
            var model = new CreateDebtTogetherViewModel
            {
                Debts = partners.Select(p => new CreateDebtTogetherViewModel.DebtItem
                {
                    PartnerId = p.PartnerId,
                    PartnerName = p.PartnerName,
                    Amount = 0
                }).ToList()
            };
            ViewBag.Partners = partners;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDebtTogether(CreateDebtTogetherViewModel model)
        {

            var debts = new List<Debt>();

            var totalAmount = model.Debts.Sum(d => d.Amount);
            var payerId = model.PayerPartnerId;
            var userId = GetCurrentUserId();
            foreach (var item in model.Debts)
            {
                if (item.Amount <= 0) continue;

                if (model.InDebt)
                {
                    // Tôi nợ người trả
                    debts.Add(new Debt
                    {
                        PartnerId = item.PartnerId,
                        Amount = item.Amount * 1000,
                        InDebt = true, // tôi nợ họ
                        DebtDate = model.DebtDate,
                        Description = $"[Nhận tiền] {model.Description}",
                        UserId = userId
                    });
                }
                else if (payerId == null || item.PartnerId != payerId)
                {
                    // Các đối tác khác nợ người trả
                    debts.Add(new Debt
                    {
                        PartnerId = item.PartnerId,
                        Amount = item.Amount * 1000,
                        InDebt = false, // họ nợ tôi hoặc người trả
                        DebtDate = model.DebtDate,
                        Description = $"[Chia nợ] {model.Description}",
                        UserId = userId
                    });
                }
            }

            if (payerId != null && !model.InDebt)
            {
                // Tôi nợ người trả phần còn lại
                var myDebtAmount = model.Debts.Where(d => d.PartnerId != payerId).Sum(d => d.Amount);
                debts.Add(new Debt
                {
                    PartnerId = payerId.Value,
                    Amount = myDebtAmount * 1000,
                    InDebt = true, // tôi nợ họ
                    DebtDate = model.DebtDate,
                    Description = $"[Chia nợ - tổng] {model.Description}",
                    UserId = userId
                });
            }

            _context.Debts.AddRange(debts);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }



        [HttpGet]
        public IActionResult CreateDebt()
        {
            var viewModel = new CreateDebtViewModel
            {
                DebtDate = DateTime.Today,
               
            };

            var userId = GetCurrentUserId();
            ViewBag.Partners = _context.Partners
                .Where(p => p.UserId == userId)
                .ToList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDebt(CreateDebtViewModel model)
        {
            var userId = GetCurrentUserId(); // Viết hàm lấy userId từ Claims

            if (!ModelState.IsValid)
            {

                ViewBag.Partners = _context.Partners
                    .Where(p => p.UserId == userId)
                    .ToList();

                return View(model);
            }


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
                .Where(p => p.UserId == GetCurrentUserId())
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
            {
                ViewBag.Partners = _context.Partners
                    .Where(p => p.UserId == GetCurrentUserId())
                    .ToList();
                if (model.PartnerId == 0 || model.Amount <= 0)
                {
                    ViewBag.ErrorMessage = "Vui lòng chọn đối tác và nhập số tiền hợp lệ.";
                }
                return View(model);
            }

            var payDebt = new PayDebt
            {
                PartnerId = model.PartnerId,
                UserId = GetCurrentUserId(), // bạn cần xử lý theo context
                Amount = model.Amount * 1000,
                Description = model.Description,
                PaymentDate = model.PaymentDate,
                InDebt = model.InDebt,
                CreatedAt = DateTime.Now
            };

            _context.PayDebts.Add(payDebt);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Debt");
        }

        [HttpGet]
        public IActionResult EditDebt(long id)
        {
            var debt = _context.Debts.FirstOrDefault(d => d.DebtId == id);
            if (debt == null) return NotFound();

            var viewModel = new CreateDebtViewModel
            {
                PartnerId = debt.PartnerId,
                InDebt = debt.InDebt,
                Amount = debt.Amount, 
                DebtDate = debt.DebtDate,
                Description = debt.Description
            };

            var userId = GetCurrentUserId();
            ViewBag.Partners = _context.Partners
                .Where(p => p.UserId == userId)
                .ToList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditDebt(long id, CreateDebtViewModel model)
        {
            if (model.PartnerId == 0 && model.Amount <= 0)
            {
                var userId = GetCurrentUserId();
                ViewBag.Partners = _context.Partners
                    .Where(p => p.UserId == userId)
                    .ToList();
                ViewBag.ErrorMessage = "Vui lòng chọn đối tác và nhập số tiền hợp lệ.";
                return View(model);
            }

            var debt = _context.Debts.FirstOrDefault(d => d.DebtId == id);
            if (debt == null) return NotFound();

            debt.PartnerId = model.PartnerId;
            debt.InDebt = model.InDebt;
            debt.Amount = model.Amount;
            debt.DebtDate = model.DebtDate;
            debt.Description = model.Description;
            // Không thay đổi CreatedDate hoặc UserId ở đây

            _context.SaveChanges();

            return RedirectToAction("DebtHistory", "Debt", new { partnerId = debt.PartnerId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDebt(long id)
        {
            var debt = await _context.Debts.FindAsync(id);
            if (debt == null)
            {
                return NotFound();
            }
            // Kiểm tra quyền sở hữu
            if (debt.UserId != GetCurrentUserId())
            {
                return Forbid();
            }
            _context.Debts.Remove(debt);
            await _context.SaveChangesAsync();
            return Json(new {status=true, message="Xóa thành công!"});
        }

        [HttpGet]
        public IActionResult EditPayDebt(long id)
        {
            var payDebt = _context.PayDebts.FirstOrDefault(p => p.PayDebtId == id);
            if (payDebt == null) return NotFound();

            var viewModel = new PayDebtViewModel
            {
                PartnerId = payDebt.PartnerId,
                Amount = payDebt.Amount,
                Description = payDebt.Description,
                PaymentDate = payDebt.PaymentDate,
                InDebt = payDebt.InDebt
            };

            var userId = GetCurrentUserId();
            ViewBag.Partners = _context.Partners
                .Where(p => p.UserId == userId)
                .ToList();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPayDebt(long id, PayDebtViewModel model)
        {
            if (model.PartnerId == 0 && model.Amount <= 0)
            {
                var userId = GetCurrentUserId();
                ViewBag.Partners = _context.Partners
                    .Where(p => p.UserId == userId)
                    .ToList();
                ViewBag.ErrorMessage = "Vui lòng chọn đối tác và nhập số tiền hợp lệ.";
                return View(model);
            }

            var payDebt = await _context.PayDebts.FindAsync(id);
            if (payDebt == null) return NotFound();

            payDebt.PartnerId = model.PartnerId;
            payDebt.Amount = model.Amount;
            payDebt.Description = model.Description;
            payDebt.PaymentDate = model.PaymentDate;
            payDebt.InDebt = model.InDebt;
            // Không thay đổi CreatedAt hoặc UserId

            await _context.SaveChangesAsync();

            return RedirectToAction("DebtHistory", "Debt", new {partnerId=payDebt.PartnerId});
        }


        [HttpPost]
        public async Task<IActionResult> DeletePayDebt(long id)
        {
            var payDebt = await _context.PayDebts.FindAsync(id);
            if (payDebt == null)
            {
                return NotFound();
            }
            // Kiểm tra quyền sở hữu
            if (payDebt.UserId != GetCurrentUserId())
            {
                return Forbid();
            }
            _context.PayDebts.Remove(payDebt);
            await _context.SaveChangesAsync();
            return Json(new { status = true, message = "Xóa thành công!" });
        }




        private long GetCurrentUserId()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return userId;
        }
    
    
    
    
    }
}
