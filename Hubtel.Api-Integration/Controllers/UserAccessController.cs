using dbContex.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hubtel.Api_Integration.Controllers
{
    public class UserAccessController : Controller
    {

        private readonly HubtelWalletDbContextExtended _context;

        public UserAccessController(HubtelWalletDbContextExtended context)
        {
            _context = context;
        }






    }
}
