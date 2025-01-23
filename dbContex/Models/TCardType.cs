using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TCardType
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TCardAccountDetail> TCardAccountDetails { get; set; } = new List<TCardAccountDetail>();
}
