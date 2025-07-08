using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;
using QuanLyChiTieu.ViewModels;
using System.Security.Claims;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class JarInfoController : Controller
    {
        private readonly QlchiTieuContext _context;
        public JarInfoController(QlchiTieuContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetJarList()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized();
            }
            var jars = _context.ExpenseJars
                .Where(j => j.UserId == userId)
                .OrderBy(j => j.JarId)
                .Select(j => new 
                {
                    j.JarId,
                    j.JarName,
                })
                .ToList();
            return Json(new { data = jars });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ExpenseJar/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateExpenseJarViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var jar = new ExpenseJar
                {
                    JarName = model.JarName,
                    UserId = userId
                };

                _context.ExpenseJars.Add(jar);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var jar = await _context.ExpenseJars
                .Where(j => j.JarId == id && j.UserId == userId)
                .FirstOrDefaultAsync();

            if (jar == null)
            {
                return NotFound();
            }

            var model = new CreateExpenseJarViewModel
            {
                JarId = jar.JarId,
                JarName = jar.JarName
            };

            return View(model);
        }

        // POST: ExpenseJar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, CreateExpenseJarViewModel model)
        {
            if (id != model.JarId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var jar = await _context.ExpenseJars
                    .Where(j => j.JarId == id && j.UserId == userId)
                    .FirstOrDefaultAsync();

                if (jar == null)
                    return NotFound();

                jar.JarName = model.JarName;
                _context.ExpenseJars.Update(jar);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var jar = await _context.ExpenseJars
                .Include(j => j.Expenses)
                .Include(j => j.IncomeAllocations)
                .Where(j => j.JarId == id && j.UserId == userId)
                .FirstOrDefaultAsync();
            if (jar == null)
            {
                return NotFound();
            }
            if (jar.Expenses.Any() || jar.IncomeAllocations.Any())
            {
                return Json(new { status = false, message = "Không thể xóa hũ chi tiêu này vì nó có liên kết với các giao dịch." });
            }
            _context.ExpenseJars.Remove(jar);
            await _context.SaveChangesAsync();
            return Json(new { status = true, message = "Hũ chi tiêu đã được xóa thành công." });
        }
    }
}
