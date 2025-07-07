using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class IncomeAllocation
{
    public long AllocationId { get; set; }

    public long IncomeId { get; set; }

    public long JarId { get; set; }

    public decimal Amount { get; set; }

    public virtual Income Income { get; set; } = null!;

    public virtual ExpenseJar Jar { get; set; } = null!;
}
