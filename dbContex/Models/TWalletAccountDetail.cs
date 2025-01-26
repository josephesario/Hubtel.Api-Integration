using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TWalletAccountDetail
{
    public Guid Id { get; set; }

    public Guid UserAccessId { get; set; }

    public Guid AccountTypeId { get; set; }

    public Guid UserProfileId { get; set; }

    public Guid SimCardTypeId { get; set; }

    public Guid CardTypeId { get; set; }

    public string AccountNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual TType AccountType { get; set; } = null!;

    public virtual TCardType CardType { get; set; } = null!;

    public virtual TSimcardType SimCardType { get; set; } = null!;

    public virtual TUserAccess UserAccess { get; set; } = null!;

    public virtual TUserProfile UserProfile { get; set; } = null!;
}
