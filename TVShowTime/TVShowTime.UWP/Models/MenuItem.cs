using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TVShowTime.UWP.Models
{
    public abstract class MenuItem
    {
        public string Name { get; set; }
        public MenuItemType Type { get; set; }
    }

    public abstract class SymbolMenuItem : MenuItem
    {
        public Symbol Symbol { get; set; }
    }

    public abstract class GlyphMenuItem : MenuItem
    {
        public string Glyph { get; set; }
    }
}
