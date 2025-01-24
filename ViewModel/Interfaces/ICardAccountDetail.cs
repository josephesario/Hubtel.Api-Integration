using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Interfaces
{

    public interface ICardAccountDetailOut : ICardAccountDetail
    {
        public DateTime? CreatedAt { get; set; }
    }

    public interface ICardAccountDetail
    {

        public string ProfileLegalName { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }

    }

}
