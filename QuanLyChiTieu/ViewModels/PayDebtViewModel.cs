namespace QuanLyChiTieu.ViewModels
{
    public class PayDebtViewModel
    {
        public long PartnerId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public decimal NetDebtAmount { get; set; } // Âm: bạn đang nợ, Dương: họ nợ bạn
        public bool InDebt { get; set; } // true: bạn thu, false: bạn trả
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
    }
}
