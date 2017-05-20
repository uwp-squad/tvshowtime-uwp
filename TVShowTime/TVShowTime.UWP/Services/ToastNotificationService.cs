using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace TVShowTime.UWP.Services
{
    public interface IToastNotificationService
    {
        void ShowErrorNotification(string text);
    }

    public class ToastNotificationService : IToastNotificationService
    {
        public void ShowErrorNotification(string text)
        {
            // Create content of the toast notification
            var toastContent = new ToastContent()
            {
                Scenario = ToastScenario.Default,
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = text
                            }
                        }
                    }
                }
            };

            // Create toast notification
            var toastNotification = new ToastNotification(toastContent.GetXml());

            // Show toast notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }
    }
}
