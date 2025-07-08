namespace QuanLyChiTieu.ViewModels
{
    public class EatingDayStat
    {
        public DateTime Date { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal AveragePerDay { get; set; }
        public decimal Difference => AveragePerDay - SpentAmount;
        public bool IsOver => Difference > 0;
    }
}
