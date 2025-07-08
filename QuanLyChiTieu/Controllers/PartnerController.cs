using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.Controllers
{
    [Authorize]
    public class PartnerController : Controller
    {
        private readonly QlchiTieuContext _context;
        public PartnerController(QlchiTieuContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> QuickCreate([FromBody] PartnerQuickCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PartnerName))
                return BadRequest("Tên đối tác không hợp lệ.");

            var partner = new Partner
            {
                PartnerName = dto.PartnerName,
                Description = ""
            };
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            return Json(new { partnerId = partner.PartnerId, partnerName = partner.PartnerName });
        }

        public class PartnerQuickCreateDto
        {
            public string PartnerName { get; set; } = "";
        }

    }
}
