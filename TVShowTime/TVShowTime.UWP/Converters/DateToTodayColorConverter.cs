using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TVShowTime.UWP.Converters
{
    public class DateToTodayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTime)value;
            var today = DateTime.Today;

            if (date.Day == today.Day && date.Month == today.Month && date.Year == today.Year)
            {
                return App.Current.Resources["PrimaryYellow"];
            }

            return App.Current.Resources["PrimaryGray"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
