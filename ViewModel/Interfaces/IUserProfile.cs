using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Interfaces
{
    public interface  IUserProfile
    {

        public string LegalName { get; set; }

        public string IdentityCardNumber { get; set; }

        public string EmailPhone { get; set; } 

    }
}
