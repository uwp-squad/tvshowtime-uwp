using TVShowTime.UWP.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TVShowTime.UWP.Views
{
    public sealed partial class ExplorePage : Page
    {
        #region Properties

        public ExploreViewModel ViewModel { get; }

        #endregion

        #region Constructor

        public ExplorePage()
        {
            InitializeComponent();

            ViewModel = (ExploreViewModel)DataContext;
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        #endregion
    }
}
