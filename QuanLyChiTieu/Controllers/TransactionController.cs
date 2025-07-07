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
    public class TransactionController : Controller
    {
        private readonly QlchiTieuContext _context;
        public TransactionController(QlchiTieuContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewByJar(long jarId)
        {
            var jar = _context.ExpenseJars
                .Where(j => j.JarId == jarId)
                .FirstOrDefault();
            return View(jar);
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(int draw, long? jarId = 0, string keyword = "")
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var incomes = await _context.Incomes
                .Where(i => i.UserId == userId)
                .Select(i => new TransactionViewModel
                {
                    Id = i.IncomeId,
                    Type = "Thu Nhập",
                    Date = i.IncomeDate.ToDateTime(TimeOnly.MinValue),
                    CreatedAt = i.CreatedAt,
                    Amount = i.TotalAmount,
                    Description = i.Description ?? "",
                    Allocations = i.IncomeAllocations
                        .Select(ia => new AllocationDetailViewModel
                        {
                            JarName = ia.Jar.JarName,
                            Amount = ia.Amount
                        })
                        .ToList(),
                    JarNames = string.Join(", ", i.IncomeAllocations.Select(ia => ia.Jar.JarName))
                })
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.Jar.UserId == userId)
                .Select(e => new TransactionViewModel
                {
                    Id = e.ExpenseId,
                    Type = "Chi Tiêu",
                    Date = e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                    CreatedAt = e.CreatedAt,
                    Amount = e.Amount,
                    Description = e.Description ?? "",
                    Allocations = null,
                    JarNames = e.Jar.JarName
                })
                .ToListAsync();

            var all = incomes.Concat(expenses)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return Json(new { draw, data = all });
        }



        [HttpGet]
        public async Task<IActionResult> GetTransactionByJar(long jarId)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Thu nhập: Lấy những income nào có phân bổ vào hũ này
            var incomes = await _context.IncomeAllocations
                .Where(ia => ia.JarId == jarId && ia.Income.UserId == userId)
                .Select(ia => new TransactionViewModel
                {
                    Type = "Thu Nhập",
                    Date = ia.Income.IncomeDate.ToDateTime(TimeOnly.MinValue),
                    Amount = ia.Amount,
                    CreatedAt = ia.Income.CreatedAt,
                    Description = ia.Income.Description ?? ""
                })
                .ToListAsync();

            // Chi tiêu: Lấy chi tiêu trong hũ
            var expenses = await _context.Expenses
                .Where(e => e.JarId == jarId && e.Jar.UserId == userId)
                .Select(e => new TransactionViewModel
                {
                    Type = "Chi Tiêu",
                    Date = e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                    CreatedAt = e.CreatedAt,
                    Amount = e.Amount,
                    Description = e.Description ?? ""
                })
                .ToListAsync();

            var all = incomes
                .Concat(expenses)
                .OrderByDescending(x => x.Date)
                    .ThenByDescending(x => x.CreatedAt)
                .ToList();

            return Json(new { data = all });
        }


        public IActionResult CreateExpense()
        {
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var model = new CreateExpenseViewModel
            {
                ExpenseDate = DateOnly.FromDateTime(DateTime.Now),
                Jars = _context.ExpenseJars
                    .Where(j => j.UserId == userId)
                    .Select(j => new SelectListItem
                    {
                        Value = j.JarId.ToString(),
                        Text = j.JarName
                    }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(CreateExpenseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var expense = new Expense
                {
                    JarId = model.JarId,
                    ExpenseDate = model.ExpenseDate,
                    Amount = model.Amount * 1000,
                    CreatedAt = DateTime.Now,
                    Description = model.Description
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Nếu lỗi validate, nạp lại danh sách hũ
            model.Jars = _context.ExpenseJars
                .Select(j => new SelectListItem
                {
                    Value = j.JarId.ToString(),
                    Text = j.JarName
                }).ToList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditExpense(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var expense = await _context.Expenses
                .Include(e => e.Jar)
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.Jar.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            var model = new CreateExpenseViewModel
            {
                ExpenseId = expense.ExpenseId, // bạn cần thêm property này vào ViewModel
                JarId = expense.JarId,
                ExpenseDate = expense.ExpenseDate,
                Amount = expense.Amount,
                Description = expense.Description,
                Jars = await _context.ExpenseJars
                    .Where(j => j.UserId == userId)
                    .Select(j => new SelectListItem
                    {
                        Value = j.JarId.ToString(),
                        Text = j.JarName,
                        Selected = j.JarId == expense.JarId
                    })
                    .ToListAsync()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExpense(long id, CreateExpenseViewModel model)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (!ModelState.IsValid)
            {
                model.Jars = await _context.ExpenseJars
                    .Where(j => j.UserId == userId)
                    .Select(j => new SelectListItem
                    {
                        Value = j.JarId.ToString(),
                        Text = j.JarName,
                        Selected = j.JarId == model.JarId
                    })
                    .ToListAsync();
                return View(model);
            }

            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == id && e.Jar.UserId == userId);

            if (expense == null)
            {
                return NotFound();
            }

            expense.JarId = model.JarId;
            expense.ExpenseDate = model.ExpenseDate;
            expense.Amount = model.Amount;
            expense.Description = model.Description;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }



        public IActionResult CreateIncome()
        {
            long userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var jars = _context.ExpenseJars
                .Where(j => j.UserId == userId)
                .Select(j => new JarAllocationViewModel
                {
                    JarId = j.JarId,
                    JarName = j.JarName
                })
                .ToList();

            var model = new CreateIncomeViewModel
            {
                IncomeDate = DateOnly.FromDateTime(DateTime.Today),
                Allocations = jars
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIncome(CreateIncomeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var income = new Income
                {
                    UserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    IncomeDate = model.IncomeDate,
                    TotalAmount = model.TotalAmount * 1000,
                    Description = model.Description,
                    CreatedAt = DateTime.Now,
                    IncomeAllocations = model.Allocations
                        .Where(a => a.Amount > 0)
                        .Select(a => new IncomeAllocation
                        {
                            JarId = a.JarId,
                            Amount = a.Amount * 1000
                        }).ToList()
                };

                _context.Incomes.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Nếu lỗi, load lại danh sách hũ
            model.Allocations = _context.ExpenseJars
                .Select(j => new JarAllocationViewModel
                {
                    JarId = j.JarId,
                    JarName = j.JarName
                }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditIncome(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var income = await _context.Incomes
                .Include(i => i.IncomeAllocations)
                    .ThenInclude(a => a.Jar)
                .FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);

            if (income == null)
                return NotFound();

            var jars = await _context.ExpenseJars
                .Where(j => j.UserId == userId)
                .ToListAsync();

            var model = new CreateIncomeViewModel
            {
                IncomeId = income.IncomeId, // Bạn cần thêm property này vào ViewModel
                IncomeDate = income.IncomeDate,
                TotalAmount = income.TotalAmount,
                Description = income.Description,
                Allocations = jars.Select(j => new JarAllocationViewModel
                {
                    JarId = j.JarId,
                    JarName = j.JarName,
                    Amount = income.IncomeAllocations.FirstOrDefault(a => a.JarId == j.JarId)?.Amount ?? 0
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIncome(long id, CreateIncomeViewModel model)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (!ModelState.IsValid)
            {
                var jars = await _context.ExpenseJars
                    .Where(j => j.UserId == userId)
                    .ToListAsync();

                model.Allocations = jars.Select(j => new JarAllocationViewModel
                {
                    JarId = j.JarId,
                    JarName = j.JarName,
                    Amount = model.Allocations.FirstOrDefault(a => a.JarId == j.JarId)?.Amount ?? 0
                }).ToList();

                return View(model);
            }

            var income = await _context.Incomes
                .Include(i => i.IncomeAllocations)
                .FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);

            if (income == null)
                return NotFound();

            // Update Income
            income.IncomeDate = model.IncomeDate;
            income.TotalAmount = model.TotalAmount;
            income.Description = model.Description;

            // Update Allocations
            foreach (var allocation in model.Allocations)
            {
                var existing = income.IncomeAllocations.FirstOrDefault(a => a.JarId == allocation.JarId);
                if (existing != null)
                {
                    existing.Amount = allocation.Amount;
                }
                else
                {
                    income.IncomeAllocations.Add(new IncomeAllocation
                    {
                        JarId = allocation.JarId,
                        Amount = allocation.Amount
                    });
                }
            }

            // Remove allocations that are no longer present
            var jarIds = model.Allocations.Select(a => a.JarId).ToHashSet();

            var notRemovedAllocations = income.IncomeAllocations
                .Where(a => jarIds.Contains(a.JarId))
                .ToList();
            income.IncomeAllocations = notRemovedAllocations;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteExpense(long id)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var expense = await _context.Expenses
                    .FirstOrDefaultAsync(e => e.ExpenseId == id && e.Jar.UserId == userId);
                if (expense == null)
                {
                    return NotFound();
                }
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if needed
                return Json(new { status = false, message = "Không thể xóa chi tiêu này do có phân bổ vào hũ." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIncome(long id)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var income = await _context.Incomes
                    .FirstOrDefaultAsync(i => i.IncomeId == id && i.UserId == userId);
                if (income == null)
                {
                    return NotFound();
                }
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) if needed
                return Json(new { status = false, message = "Không thể xóa thu nhập này do có phân bổ vào hũ." });
            }

        }
    }
}
