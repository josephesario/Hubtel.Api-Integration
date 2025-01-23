using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TUserAccess
{
    public Guid Id { get; set; }

    public Guid UserTypeId { get; set; }

    public string EmailPhoneNumber { get; set; } = null!;

    public string? UserSecret { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TCardAccountDetail> TCardAccountDetails { get; set; } = new List<TCardAccountDetail>();

    public virtual ICollection<TPhoneAccountDetail> TPhoneAccountDetails { get; set; } = new List<TPhoneAccountDetail>();

    public virtual ICollection<TUserProfile> TUserProfiles { get; set; } = new List<TUserProfile>();

    public virtual TUserType UserType { get; set; } = null!;
}
