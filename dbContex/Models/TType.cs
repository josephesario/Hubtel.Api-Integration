using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TType
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TWalletAccountDetail> TWalletAccountDetails { get; set; } = new List<TWalletAccountDetail>();
}
