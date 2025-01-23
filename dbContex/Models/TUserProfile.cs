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

    public string PhoneNumber { get; set; } = null!;

    public virtual ICollection<TCardAccountDetail> TCardAccountDetails { get; set; } = new List<TCardAccountDetail>();

    public virtual ICollection<TPhoneAccountDetail> TPhoneAccountDetails { get; set; } = new List<TPhoneAccountDetail>();

    public virtual TUserAccess UserAccess { get; set; } = null!;
}
