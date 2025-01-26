using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TUserAccess
{
    public Guid Id { get; set; }

    public string EmailPhoneNumber { get; set; } = null!;

    public string? UserSecret { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TUserProfile> TUserProfiles { get; set; } = new List<TUserProfile>();

    public virtual ICollection<TWalletAccountDetail> TWalletAccountDetails { get; set; } = new List<TWalletAccountDetail>();
}
