using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class User
{
    public long UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Debt> Debts { get; set; } = new List<Debt>();

    public virtual ICollection<ExpenseJar> ExpenseJars { get; set; } = new List<ExpenseJar>();

    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();

    public virtual ICollection<PayDebt> PayDebts { get; set; } = new List<PayDebt>();
}
