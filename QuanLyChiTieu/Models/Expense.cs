using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class Expense
{
    public long ExpenseId { get; set; }

    public long JarId { get; set; }

    public DateOnly ExpenseDate { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public virtual ExpenseJar Jar { get; set; } = null!;
}
