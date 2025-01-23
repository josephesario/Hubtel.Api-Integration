using System;
using System.Collections.Generic;


namespace ViewModel.Data;

public partial class TPhoneAccountDetail
{
    public Guid UserAccessId { get; set; }

    public Guid UserProfileId { get; set; }

    public Guid SimCardTypeId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

}
