namespace QuanLyChiTieu.ViewModels
{
    public class JarViewModel
    {
        public long JarId { get; set; }
        public string JarName { get; set; }
        public decimal TotalAmount { get; set; }
        // Constructor to initialize the properties
        public JarViewModel(long jarId, string jarName, decimal totalAmount)
        {
            JarId = jarId;
            JarName = jarName;
            TotalAmount = totalAmount;
        }

        public JarViewModel() 
        {
            JarId = 0;
            JarName = string.Empty;
            TotalAmount = 0.0m;
        }
    }
}
