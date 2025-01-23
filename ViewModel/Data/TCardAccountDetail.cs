using System;
using System.Collections.Generic;


namespace ViewModel.Data;

public partial class TCardAccountDetail
{

    public Guid UserAccessId { get; set; }

    public Guid UserProfileId { get; set; }

    public Guid CardTypeId { get; set; }

    public string CardNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

}
