using System;
using TVShowTimeApi.Model;
using Windows.UI.Xaml.Data;

namespace TVShowTime.UWP.Converters
{
    public class EmotionValueToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null)
                return false;

            var emotion = (Emotion)value;
            var parameterEmotion = (Emotion)parameter;

            return (emotion == parameterEmotion);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
