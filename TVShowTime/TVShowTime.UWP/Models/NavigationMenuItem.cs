using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShowTime.UWP.Models
{
    public class NavigationMenuItem : SymbolMenuItem
    {
        public Type PageType { get; set; }
    }
}
