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
        public async Task<IActionResult> Index()
        {
            var tienAnId = await GetEatingId();
            ViewBag.TienAnId = tienAnId;
            return View();
        }

        public async Task<IActionResult> GetDayLy(int month, int year)
        {
            var userId = GetCurrentUserId();
            var tienAnId = await GetEatingId();
            ViewBag.TienAnId = tienAnId;
            var totalEatingMoney = await _context.IncomeAllocations
                .Include(i => i.Income)
                .Where(i => i.JarId == tienAnId &&
                            i.Income.IncomeDate.Month == month &&
                            i.Income.IncomeDate.Year == year &&
                            i.Income.UserId == userId)
                .SumAsync(i => i.Amount);

            var expenses = await _context.Expenses
                .Include(e => e.Jar)
                .Where(e => e.JarId == tienAnId &&
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

        private async Task<long> GetEatingId()
        {
            var userId = GetCurrentUserId();
            var tienAnId = await _context.ExpenseJars
                .Where(j => j.JarName.ToLower() == "tiền ăn" && j.UserId == userId)
                .Select(j => j.JarId)
                .FirstOrDefaultAsync();
            if (tienAnId == 0)
            {
                var newJar = new ExpenseJar
                {
                    JarName = "Tiền ăn",
                    UserId = GetCurrentUserId()
                };
                _context.ExpenseJars.Add(newJar);
                await _context.SaveChangesAsync();
                tienAnId = newJar.JarId;
            }
            return tienAnId;
        }

        private long GetCurrentUserId()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return userId;
        }
    }
}
