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
        public async Task<IActionResult> GetReportData(long? jarId, int month, int year)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            var totalDays = DateTime.DaysInMonth(year, month);

            var incomeQuery = _context.IncomeAllocations
                .Include(i => i.Income)
                .Include(i => i.Jar)
                .Where(i => i.Income.UserId == userId &&
                            i.Income.IncomeDate >= DateOnly.FromDateTime(startDate) &&
                            i.Income.IncomeDate < DateOnly.FromDateTime(endDate));

            var expenseQuery = _context.Expenses
                .Include(e => e.Jar)
                .Where(e => e.Jar.UserId == userId &&
                            e.ExpenseDate >= DateOnly.FromDateTime(startDate) &&
                            e.ExpenseDate < DateOnly.FromDateTime(endDate));

            if (jarId.HasValue)
            {
                incomeQuery = incomeQuery.Where(i => i.JarId == jarId.Value);
                expenseQuery = expenseQuery.Where(e => e.JarId == jarId.Value);
            }

            var incomes = await incomeQuery.ToListAsync();
            var expenses = await expenseQuery.ToListAsync();

            // Tổng hợp thu nhập theo ngày
            var incomeByDay = incomes
                .GroupBy(i => i.Income.IncomeDate.Day)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));

            // Tổng hợp chi tiêu theo ngày
            var expenseByDay = expenses
                .GroupBy(e => e.ExpenseDate.Day)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

            var dailyStats = new List<object>();
            decimal cumulativeIncome = 0;
            decimal cumulativeExpense = 0;

            for (int day = 1; day <= totalDays; day++)
            {
                var incomeToday = incomeByDay.TryGetValue(day, out var inc) ? inc : 0m;
                var expenseToday = expenseByDay.TryGetValue(day, out var exp) ? exp : 0m;

                cumulativeIncome += incomeToday;
                cumulativeExpense += expenseToday;
                var remain = cumulativeIncome - cumulativeExpense;

                dailyStats.Add(new
                {
                    date = new DateTime(year, month, day).ToString("dd/MM"),
                    income = incomeToday,
                    expense = expenseToday,
                    remain = remain
                });
            }

            // Pie chart
            var incomePie = incomes
                .GroupBy(i => i.Jar.JarName)
                .Select(g => new
                {
                    label = g.Key,
                    value = g.Sum(i => i.Amount)
                })
                .ToList();

            var expensePie = expenses
                .GroupBy(e => e.Jar.JarName)
                .Select(g => new
                {
                    label = g.Key,
                    value = g.Sum(e => e.Amount)
                })
                .ToList();

            return Json(new
            {
                daily = dailyStats,
                incomePie,
                expensePie
            });
        }



    }
}
