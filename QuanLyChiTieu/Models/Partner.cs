using System;
using System.Collections.Generic;

namespace QuanLyChiTieu.Models;

public partial class Partner
{
    public long PartnerId { get; set; }

    public string PartnerName { get; set; } = null!;

    public string? Description { get; set; }

    public long? UserId { get; set; }

    public virtual ICollection<Debt> Debts { get; set; } = new List<Debt>();

    public virtual ICollection<PayDebt> PayDebts { get; set; } = new List<PayDebt>();
}
