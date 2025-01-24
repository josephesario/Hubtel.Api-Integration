using System;
using System.Collections.Generic;
using ViewModel.Interfaces;


namespace ViewModel.Data;

public partial class CardType: ICardType
{
    public string? Name { get; set; }

}
