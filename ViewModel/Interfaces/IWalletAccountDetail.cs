using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Interfaces
{



    public interface IWalletAccountDetail
    {

        public string ProfileLegalName { get; set; }
        public string? AccountScheme { get; set; }
        public string? AccountNumber { get; set; }

    }

    public interface IWalletAccountDetailOut : IWalletAccountDetail
    {
        public Guid Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? AccountType { get; set; }
    }

}
