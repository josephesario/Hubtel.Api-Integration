using System;
using System.Collections.Generic;


namespace ViewModel.Data;

public partial class TUserAccess
{

    public Guid UserTypeId { get; set; }

    public string EmailPhoneNumber { get; set; } = null!;

    public string? UserSecret { get; set; }

    public DateTime? CreatedAt { get; set; }

}
