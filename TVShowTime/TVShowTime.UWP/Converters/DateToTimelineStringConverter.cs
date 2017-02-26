using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TVShowTime.UWP.Converters
{
    public class DateToTimelineStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTime)value;
            var today = DateTime.Today;

            var pureDate = new DateTime(date.Year, date.Month, date.Day);
            var pureToday = new DateTime(today.Year, today.Month, today.Day);

            if (pureDate == pureToday)
            {
                return "Today";
            }

            int daysBeforeToday = (int)pureToday.Subtract(pureDate).TotalDays;
            if (daysBeforeToday > 0)
            {
                if (daysBeforeToday == 1)
                {
                    return "Yesterday";
                }

                return $"{daysBeforeToday} days ago";
            }

            int daysAfterToday = -daysBeforeToday;
            if (daysAfterToday == 1)
            {
                return "Tomorrow";
            }

            return $"In {daysAfterToday} days";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
