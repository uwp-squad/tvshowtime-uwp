using System;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class UserConnnectionProfile
    {
        public User User { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
