using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Linq;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Models;
using TVShowTime.UWP.Services;
using TVShowTime.UWP.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TVShowTime.UWP.Views
{
    public sealed partial class MainPage : Page
    {
        #region Fields

        private IHamburgerMenuService _hamburgerMenuService;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            // Retrieve colors from app resources
            var primaryBlackColor = (App.Current.Resources["PrimaryBlack"] as SolidColorBrush).Color;
            var primaryWhiteColor = (App.Current.Resources["PrimaryWhite"] as SolidColorBrush).Color;
            var primaryYellowColor = (App.Current.Resources["PrimaryYellow"] as SolidColorBrush).Color;

            // Style title bar (Desktop)
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.ButtonBackgroundColor = primaryBlackColor;
                    titleBar.ButtonForegroundColor = primaryWhiteColor;

                    titleBar.ButtonHoverBackgroundColor = primaryYellowColor;
                    titleBar.ButtonHoverForegroundColor = primaryBlackColor;

                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(
                        primaryYellowColor.A,
                        primaryYellowColor.R,
                        primaryYellowColor.G,
                        (byte)(primaryYellowColor.B + 100));
                    titleBar.ButtonPressedForegroundColor = primaryBlackColor;

                    titleBar.BackgroundColor = primaryBlackColor;
                    titleBar.ForegroundColor = primaryWhiteColor;
                }
            }

            // Style status bar (Mobile)
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = primaryBlackColor;
                    statusBar.ForegroundColor = primaryWhiteColor;
                }
            }

            // Register a handler for BackRequested events
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Manage hamburger menu service
            _hamburgerMenuService = ServiceLocator.Current.GetInstance<IHamburgerMenuService>();

            HamburgerMenuControl.ItemsSource = _hamburgerMenuService.MenuItems.Where(item => item.Type == MenuItemType.Main);
            HamburgerMenuControl.OptionsItemsSource = _hamburgerMenuService.MenuItems.Where(item => item.Type == MenuItemType.Options);

            // After the page is loaded
            Loaded += OnLoaded;
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                // Initialize hamburger menu service
                InitializeHamburgerNavigationService();
                _hamburgerMenuService.NavigateTo(ViewConstants.ToWatch);
            }

            // Style title bar (Desktop)
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                Window.Current.SetTitleBar(ExtendedTitleBar);
            }

            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void OnMenuItemClick(object sender, ItemClickEventArgs e)
        {
            if (SystemInformation.DeviceFamily == "Windows.Mobile")
            {
                HamburgerMenuControl.IsPaneOpen = false;
            }

            if (e.ClickedItem is INavigationMenuItem navigationMenuItem)
            {
                ContentFrame.Navigate(navigationMenuItem.PageType);
            }

            if (e.ClickedItem is ActionMenuItem actionMenuItem)
            {
                actionMenuItem.Action();
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                e.Handled = true;
                ContentFrame.GoBack();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Handle app activation
            ServiceLocator.Current.GetInstance<MainViewModel>().HandleActivation();
        }

        #endregion

        #region Methods

        private void InitializeHamburgerNavigationService()
        {
            _hamburgerMenuService.SetHamburgerMenuElement(HamburgerMenuControl);
            _hamburgerMenuService.SetFrameElement(ContentFrame);

            _hamburgerMenuService.Configure(ViewConstants.Agenda, typeof(AgendaPage));
            _hamburgerMenuService.Configure(ViewConstants.Collection, typeof(CollectionPage));
            _hamburgerMenuService.Configure(ViewConstants.Episode, typeof(EpisodePage));
            _hamburgerMenuService.Configure(ViewConstants.Explore, typeof(ExplorePage));
            _hamburgerMenuService.Configure(ViewConstants.Feedback, typeof(FeedbackPage));
            _hamburgerMenuService.Configure(ViewConstants.Settings, typeof(SettingsPage));
            _hamburgerMenuService.Configure(ViewConstants.Show, typeof(ShowPage));
            _hamburgerMenuService.Configure(ViewConstants.ToWatch, typeof(ToWatchPage));
            _hamburgerMenuService.Configure(ViewConstants.Upcoming, typeof(UpcomingPage));
        }

        #endregion
    }
}
