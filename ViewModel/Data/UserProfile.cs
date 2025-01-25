using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class UserProfile: IUserProfile
{

    [Required]
    public string LegalName { get; set; } = null!;
    [Required]
    public string IdentityCardNumber { get; set; } = null!;
    [Required]
    public string EmailPhone { get; set; } = null!;

}
