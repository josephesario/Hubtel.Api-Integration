using System;
using System.Collections.Generic;


namespace ViewModel.Interfaces;

public interface IPhoneAccountDetail
{
    public string ProfileLegalName { get; set; }
    public string SimCardType { get; set; }
    public string PhoneNumber { get; set; }
}
public interface IPhoneAccountDetailOut : IPhoneAccountDetail
{
    public DateTime? CreatedAt { get; set; }
}

