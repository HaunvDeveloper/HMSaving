using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyChiTieu.ViewModels
{
    public class CreateExpenseViewModel
    {
        public long JarId { get; set; }
        public DateOnly ExpenseDate { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public List<SelectListItem> Jars { get; set; } = new();
    }

}
