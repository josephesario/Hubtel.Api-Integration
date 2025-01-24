using System;
using System.Collections.Generic;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class CardAccountDetail: ICardAccountDetail
{

    public string ProfileLegalName { get; set; } = null!;

    public string CardType { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

}

public partial class CardAccountDetailOut: CardAccountDetail, ICardAccountDetailOut
{
    public DateTime? CreatedAt { get; set; }
}

