using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class Debt
{
    public long DebtId { get; set; }

    public long PartnerId { get; set; }

    public long UserId { get; set; }

    public bool InDebt { get; set; }

    public decimal Amount { get; set; }

    public DateTime DebtDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? Description { get; set; }

    public virtual Partner Partner { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
