using System;
using System.Collections.Generic;

namespace dbContex.Models;

public partial class TCardAccountDetail
{
    public Guid Id { get; set; }

    public Guid UserAccessId { get; set; }

    public Guid UserProfileId { get; set; }

    public Guid CardTypeId { get; set; }

    public string CardNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual TCardType CardType { get; set; } = null!;

    public virtual TUserAccess UserAccess { get; set; } = null!;

    public virtual TUserProfile UserProfile { get; set; } = null!;
}
