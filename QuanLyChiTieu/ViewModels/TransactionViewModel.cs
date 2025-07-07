namespace QuanLyChiTieu.ViewModels
{
    public class TransactionViewModel
    {
        public long Id { get; set; }
        public string Type { get; set; } = "";
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = "";
        public List<AllocationDetailViewModel>? Allocations { get; set; }
        public string JarNames { get; set; } = "";
    }

    public class AllocationDetailViewModel
    {
        public string JarName { get; set; } = "";
        public decimal Amount { get; set; }
    }

}
