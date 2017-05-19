using System;
using Windows.UI.Xaml.Data;

namespace TVShowTime.UWP.Converters
{
    public class NullableBoolInvertedToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? b = (bool?)value;
            return !(b.HasValue && b.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }
    }
}
