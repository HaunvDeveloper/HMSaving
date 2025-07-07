using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class Income
{
    public long IncomeId { get; set; }

    public long UserId { get; set; }

    public DateOnly IncomeDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<IncomeAllocation> IncomeAllocations { get; set; } = new List<IncomeAllocation>();

    public virtual User User { get; set; } = null!;
}
