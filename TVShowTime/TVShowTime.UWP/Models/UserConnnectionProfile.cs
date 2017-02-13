using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class UserConnnectionProfile
    {
        public User User { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
