using System;
using System.Collections.Generic;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class UserProfile: IUserProfile
{

    public string LegalName { get; set; } = null!;

    public string IdentityCardNumber { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

}
