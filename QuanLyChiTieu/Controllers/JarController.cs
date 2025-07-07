using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class JarController : Controller
    {

        private readonly QlchiTieuContext _context;

        public JarController(QlchiTieuContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }

        public async Task<IActionResult> _GetJarByMonth(int? month, int? year)
        {
            month ??= DateTime.Now.Month;
            year ??= DateTime.Now.Year;
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Tổng thu nhập tháng
            var totalRevenue = await _context.Incomes
                .Where(x => x.UserId == userId && x.IncomeDate.Month == month && x.IncomeDate.Year == year)
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;

            // Tổng chi tiêu tháng
            var totalCost = await _context.Expenses
                .Include(x => x.Jar)
                .Where(e => e.Jar.UserId == userId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            // Lấy danh sách hũ
            var jars = await _context.ExpenseJars
                .Where(j => j.UserId == userId)
                .Select(j => new JarViewModel
                {
                    JarId = j.JarId,
                    JarName = j.JarName,
                    TotalAmount =
                        (
                            _context.IncomeAllocations
                                .Where(ia => ia.JarId == j.JarId && ia.Income.UserId == userId && ia.Income.IncomeDate.Month == month && ia.Income.IncomeDate.Year == year)
                                .Sum(ia => (decimal?)ia.Amount) ?? 0
                        )
                        -
                        (
                            _context.Expenses
                                .Where(e => e.JarId == j.JarId && e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                                .Sum(e => (decimal?)e.Amount) ?? 0
                        )
                })
                .ToListAsync();

            // Gán ViewBag
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCost = totalCost;

            return PartialView(jars);
        }


        public IActionResult Details()
        {
            return View();
        }
    }
}
