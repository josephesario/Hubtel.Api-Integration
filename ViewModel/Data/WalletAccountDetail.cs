using System;
using System.Collections.Generic;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class WalletAccountDetail: IWalletAccountDetail
{

    public string ProfileLegalName { get; set; } = null!;

    public string CardType { get; set; } = null!;

    public string? AccountNumber { get; set; }
    public string? AccountType { get; set; }

}

public partial class WalletAccountDetailOut : WalletAccountDetail, IWalletAccountDetailOut
{
    public DateTime? CreatedAt { get; set; }
}

