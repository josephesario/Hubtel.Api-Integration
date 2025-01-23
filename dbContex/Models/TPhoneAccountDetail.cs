using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TPhoneAccountDetail
{
    public Guid Id { get; set; }

    public Guid UserAccessId { get; set; }

    public Guid UserProfileId { get; set; }

    public Guid SimCardTypeId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual TSimcardType SimCardType { get; set; } = null!;

    public virtual TUserAccess UserAccess { get; set; } = null!;

    public virtual TUserProfile UserProfile { get; set; } = null!;
}
