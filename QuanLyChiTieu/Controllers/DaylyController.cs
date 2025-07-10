using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class DaylyController : Controller
    {
        private readonly QlchiTieuContext _context;
        public DaylyController(QlchiTieuContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetDayLy(int month, int year)
        {
            var userId = GetCurrentUserId();

            var totalEatingMoney = await _context.IncomeAllocations
                .Include(i => i.Income)
                .Where(i => i.JarId == 1 &&
                            i.Income.IncomeDate.Month == month &&
                            i.Income.IncomeDate.Year == year &&
                            i.Income.UserId == userId)
                .SumAsync(i => i.Amount);

            var expenses = await _context.Expenses
                .Include(e => e.Jar)
                .Where(e => e.JarId == 1 &&
                            e.Jar.UserId == userId &&
                            e.ExpenseDate.Month == month &&
                            e.ExpenseDate.Year == year)
                .GroupBy(e => e.ExpenseDate)
                .Select(g => new { Date = g.Key, Total = g.Sum(e => e.Amount) })
                .ToDictionaryAsync(g => g.Date, g => g.Total);

            var stats = new List<EatingDayStat>();
            var daysInMonth = DateTime.DaysInMonth(year, month);
            decimal remainingMoney = totalEatingMoney;
            int remainingDays = daysInMonth;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dateOnly = DateOnly.FromDateTime(date);
                var spent = expenses.ContainsKey(dateOnly) ? expenses[dateOnly] : 0;

                var adjustedAverage = remainingMoney / remainingDays;

                stats.Add(new EatingDayStat
                {
                    Date = date,
                    SpentAmount = spent,
                    AveragePerDay = adjustedAverage
                });

                remainingMoney -= spent;
                remainingDays--;
            }

            ViewBag.TotalEatingMoney = totalEatingMoney;
            return PartialView(stats);
        }



        private long GetCurrentUserId()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return userId;
        }
    }
}
