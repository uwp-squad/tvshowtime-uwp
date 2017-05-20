using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace TVShowTime.UWP.Controls
{
    public class ToggleButtonEx
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.RegisterAttached("IsChecked", typeof(bool), typeof(ToggleButtonEx), new PropertyMetadata(false, OnChanged));

        public static bool GetIsChecked(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsCheckedProperty);
        }
        public static void SetIsChecked(DependencyObject obj, bool value)
        {
            obj.SetValue(IsCheckedProperty, value);
        }

        private static void OnChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var tb = o as ToggleButton;
            if (null != tb)
                tb.IsChecked = (bool)args.NewValue;
        }
    }
}
