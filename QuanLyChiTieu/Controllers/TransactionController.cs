using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                    TotalAmount = model.TotalAmount,
                    Description = model.Description,
                    IncomeAllocations = model.Allocations
                        .Where(a => a.Amount > 0)
                        .Select(a => new IncomeAllocation
                        {
                            JarId = a.JarId,
                            Amount = a.Amount
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

    }
}
