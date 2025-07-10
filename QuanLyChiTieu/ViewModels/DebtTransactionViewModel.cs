using QuanLyChiTieu.Models;

namespace QuanLyChiTieu.ViewModels
{
    public class DebtTransactionViewModel
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public string State { get; set; }

        public bool InDebt { get; set; }

        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? Description { get; set; }

    }
}
