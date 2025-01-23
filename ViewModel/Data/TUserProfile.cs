using System;
using System.Collections.Generic;


namespace ViewModel.Data;

public partial class TUserProfile
{

    public Guid UserAccessId { get; set; }

    public string LegalName { get; set; } = null!;

    public string IdentityCardNumber { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string PhoneNumber { get; set; } = null!;

}
