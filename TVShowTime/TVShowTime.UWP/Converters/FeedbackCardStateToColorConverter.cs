using System;
using TVShowTime.UWP.Models;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TVShowTime.UWP.Converters
{
    public class FeedbackCardStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FeedbackCardState)value;

            switch (state)
            {
                case FeedbackCardState.NeedVotes:
                    return new SolidColorBrush(Colors.CornflowerBlue);
                case FeedbackCardState.InProgress:
                    return (App.Current.Resources["PrimaryYellow"] as SolidColorBrush);
                case FeedbackCardState.Done:
                    return (App.Current.Resources["PrimaryGreen"] as SolidColorBrush);
                case FeedbackCardState.Canceled:
                    return new SolidColorBrush(Colors.Red);
                case FeedbackCardState.Removed:
                    return new SolidColorBrush(Colors.Black);
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
