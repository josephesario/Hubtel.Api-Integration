using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TUserProfile
{
    public Guid Id { get; set; }

    public Guid UserAccessId { get; set; }

    public string LegalName { get; set; } = null!;

    public string IdentityCardNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string EmailPhone { get; set; } = null!;

    public virtual ICollection<TWalletAccountDetail> TWalletAccountDetails { get; set; } = new List<TWalletAccountDetail>();

    public virtual TUserAccess UserAccess { get; set; } = null!;
}
