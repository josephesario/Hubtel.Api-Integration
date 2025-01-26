using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Interfaces
{

    public interface IWalletAccountDetailOut : IWalletAccountDetail
    {
        public DateTime? CreatedAt { get; set; }
    }

    public interface IWalletAccountDetail
    {

        public string ProfileLegalName { get; set; }
        public string CardType { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountType { get; set; }

    }

}
