using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel.Interfaces
{
    public interface IUserType
    {
        public string? Name { get; set; }

        public DateTime? CreatedAt { get; set; }

    }

}
