using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class ExpenseJar
{
    public long JarId { get; set; }

    public long UserId { get; set; }

    public string JarName { get; set; } = null!;

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<IncomeAllocation> IncomeAllocations { get; set; } = new List<IncomeAllocation>();

    public virtual User User { get; set; } = null!;
}
