namespace QuanLyChiTieu.ViewModels
{
    public class JarViewModel
    {
        public long JarId { get; set; }
        public string JarName { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal TotalAmount => TotalIncome - TotalExpense;
        // Constructor to initialize the properties
        public JarViewModel(long jarId, string jarName)
        {
            JarId = jarId;
            JarName = jarName;
        }

        public JarViewModel() 
        {
            JarId = 0;
            JarName = string.Empty;
            TotalIncome = 0.0m;
            TotalExpense = 0.0m;
        }
    }
}
