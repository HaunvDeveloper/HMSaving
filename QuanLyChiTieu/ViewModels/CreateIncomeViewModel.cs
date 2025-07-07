namespace QuanLyChiTieu.ViewModels
{
    public class CreateIncomeViewModel
    {
        public long IncomeId { get; set; }
        public DateOnly IncomeDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }

        public List<JarAllocationViewModel> Allocations { get; set; } = new();
    }

    public class JarAllocationViewModel
    {
        public long JarId { get; set; }
        public string JarName { get; set; } = "";
        public decimal Amount { get; set; }
    }

}
