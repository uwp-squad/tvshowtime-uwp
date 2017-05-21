using System;
using Windows.UI.Xaml.Data;

namespace TVShowTime.UWP.Converters
{
    public class NotEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (int)value != (int)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
