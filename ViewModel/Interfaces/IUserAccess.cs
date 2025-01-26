using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Interfaces
{
    public interface  IUserAccess
    {

        public string? EmailPhoneNumber { get; set; } 

        public string? UserSecret { get; set; }


    }

}
