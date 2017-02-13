using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShowTime.UWP.Constants
{
    public static class LoginConstants
    {
        /// <summary>
        /// Name of the resource for Password Vault
        /// </summary>
        public const string AppResource = "TVShowTime UWP";

        /// <summary>
        /// Lifetime of the Access Token to be able to retrieve data from the API (= 6 months / 180 days)
        /// </summary>
        public const double TokenLifetime = 6 * 30 * 24 * 60 * 60;
    }
}
