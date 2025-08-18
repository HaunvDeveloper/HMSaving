namespace QuanLyChiTieu.ViewModels
{
    public class CreateDebtTogetherViewModel
    {
        public List<DebtItem> Debts { get; set; } = new();
        public long? PayerPartnerId { get; set; } // null nghĩa là "Tôi" trả
        public DateTime DebtDate { get; set; } = DateTime.Today;
        public string Description { get; set; } = string.Empty;
        public bool InDebt { get; set; } // true: nợ, false: được nợ
        public string TabMode { get; set; } // "individual" hoặc "shared"
        public decimal SharedAmount { get; set; }
        public List<int> SelectedPartnerIds { get; set; }

        public class DebtItem
        {
            public long PartnerId { get; set; }
            public string PartnerName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }
    }

}
