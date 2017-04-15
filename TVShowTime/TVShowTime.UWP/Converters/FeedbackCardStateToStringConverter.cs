using System;
using TVShowTime.UWP.Models;
using Windows.UI.Xaml.Data;

namespace TVShowTime.UWP.Converters
{
    public class FeedbackCardStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var state = (FeedbackCardState)value;

            switch (state)
            {
                case FeedbackCardState.NeedVotes:
                    return "NEED VOTES";
                case FeedbackCardState.InProgress:
                    return "IN PROGRESS";
                case FeedbackCardState.Done:
                    return "DONE";
                case FeedbackCardState.Canceled:
                    return "CANCELED";
                case FeedbackCardState.Removed:
                    return "REMOVED";
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
