using System;
using System.Collections.Generic;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class AccountType: IAccountType
{
    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

}
