using System;
using ViewModel.Interfaces;

namespace ViewModel.Data
{


    public class PhoneAccountDetail : IPhoneAccountDetail
    {
        public string ProfileLegalName { get; set; } = null!;
        public string SimCardType { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }

    public class PhoneAccountDetailOut : PhoneAccountDetail, IPhoneAccountDetailOut
    {
        public DateTime? CreatedAt { get; set; }
    }


}