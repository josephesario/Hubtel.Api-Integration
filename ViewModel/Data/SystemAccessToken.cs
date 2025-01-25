using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel.Interfaces;

namespace ViewModel.Data
{
    public class SystemAccessToken: ISystemAccessToken
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
