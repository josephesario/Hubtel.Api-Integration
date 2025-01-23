using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TUserType
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TUserAccess> TUserAccesses { get; set; } = new List<TUserAccess>();
}
