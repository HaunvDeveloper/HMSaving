using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyChiTieu.ViewModels
{
    public class CreateDebtViewModel
    {
        public long PartnerId { get; set; }
        public bool InDebt { get; set; } // true = mình nợ, false = họ nợ
        public decimal Amount { get; set; }
        public DateTime DebtDate { get; set; }
        public string? Description { get; set; }

        // Dùng để hiện danh sách partner
        public List<SelectListItem> Partners { get; set; } = new();
    }

}
