using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVShowTime.UWP.Models
{
    public class ActionMenuItem : GlyphMenuItem
    {
        public Action Action { get; set; }
    }
}
