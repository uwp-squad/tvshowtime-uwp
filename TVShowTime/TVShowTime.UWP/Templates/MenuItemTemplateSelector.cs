using TVShowTime.UWP.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TVShowTime.UWP.Templates
{
    public class MenuItemTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate SymbolMenuItemTemplate { get; set; }
        public DataTemplate GlyphMenuItemTemplate { get; set; }

        #endregion

        #region Methods

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is SymbolMenuItem)
            {
                return SymbolMenuItemTemplate;
            }
            if (item is GlyphMenuItem)
            {
                return GlyphMenuItemTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }

        #endregion
    }
}
