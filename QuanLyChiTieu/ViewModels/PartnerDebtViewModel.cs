namespace QuanLyChiTieu.ViewModels
{
    public class PartnerDebtViewModel
    {
        public long PartnerId { get; set; }
        public string PartnerName { get; set; } = null!;
        public decimal NetDebtAmount { get; set; } // >0: người ta thiếu mình, <0: mình thiếu người ta
    }

}
