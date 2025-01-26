using System;
using System.Collections.Generic;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class UserAccess : Login, IUserAccess
{

}

public partial class Login: ILogin
{
    public string EmailPhoneNumber { get; set; } = null!;
    public string? UserSecret { get; set; }
}

