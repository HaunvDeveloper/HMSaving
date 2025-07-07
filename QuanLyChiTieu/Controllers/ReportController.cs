using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly QlchiTieuContext _context;
        public ReportController(QlchiTieuContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetReportData(int month, int year)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            // 1. Thu nhập và chi tiêu mỗi ngày
            var daily = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(d => new
                {
                    date = d.ToString("00") + "/" + month,
                    income = 0m,
                    expense = 0m
                })
                .ToList();

            var incomes = await _context.Incomes
                .Include(x => x.IncomeAllocations)
                .ThenInclude(ia => ia.Jar)
                .Where(i => i.UserId == userId && i.IncomeDate >= DateOnly.FromDateTime(startDate) && i.IncomeDate < DateOnly.FromDateTime(endDate))
                .ToListAsync();

            foreach (var income in incomes)
            {
                var day = income.IncomeDate.Day;
                daily[day - 1] = daily[day - 1] with { income = daily[day - 1].income + income.TotalAmount };
            }

            var expenses = await _context.Expenses
                .Include(x => x.Jar)
                .Where(e => e.Jar.UserId == userId && e.ExpenseDate >= DateOnly.FromDateTime(startDate) && e.ExpenseDate < DateOnly.FromDateTime(endDate))
                .ToListAsync();

            foreach (var expense in expenses)
            {
                var day = expense.ExpenseDate.Day;
                daily[day - 1] = daily[day - 1] with { expense = daily[day - 1].expense + expense.Amount };
            }

            // 2. Pie chart Chi tiêu
            var expensePie = expenses
                .GroupBy(e => e.Jar.JarName)
                .Select(g => new { label = g.Key, value = g.Sum(x => x.Amount) })
                .ToList();

            // 3. Pie chart Thu nhập
            var incomePie = incomes
                .SelectMany(i => i.IncomeAllocations)
                .GroupBy(a => a.Jar.JarName)
                .Select(g => new { label = g.Key, value = g.Sum(x => x.Amount) })
                .ToList();

            return Json(new
            {
                daily,
                expensePie,
                incomePie
            });
        }

    }
}
