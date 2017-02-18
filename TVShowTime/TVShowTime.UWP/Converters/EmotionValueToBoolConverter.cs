using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTimeApi.Model;
using Windows.UI.Xaml;
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
