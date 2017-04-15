using System;

namespace TVShowTime.UWP.Models
{
    public interface INavigationMenuItem
    {
        Type PageType { get; set; }
    }

    public class SymbolNavigationMenuItem : SymbolMenuItem, INavigationMenuItem
    {
        public Type PageType { get; set; }
    }

    public class GlyphNavigationMenuItem : GlyphMenuItem, INavigationMenuItem
    {
        public Type PageType { get; set; }
    }
}
