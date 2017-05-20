using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using Microsoft.Toolkit.Uwp;
using Microsoft.Practices.ServiceLocation;
using TVShowTime.UWP.BackgroundTasks;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Services;
using Windows.ApplicationModel.Background;
using TVShowTime.UWP.Views;
using System.Linq;
using TVShowTime.UWP.Models;
using System.Collections.ObjectModel;

namespace TVShowTime.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private INavigationService _navigationService;
        private IHamburgerMenuService _hamburgerMenuService;

        private IObjectStorageHelper _localObjectStorageHelper;

        #endregion

        #region Properties

        public ObservableCollection<MenuItem> MenuItems { get; } = new ObservableCollection<MenuItem>();
        public ObservableCollection<MenuItem> OptionMenuItems { get; } = new ObservableCollection<MenuItem>();

        #endregion

        #region Constructor

        public MainViewModel(
            INavigationService navigationService,
            IHamburgerMenuService hamburgerMenuService)
        {
            _navigationService = navigationService;
            _hamburgerMenuService = hamburgerMenuService;

            _localObjectStorageHelper = ServiceLocator.Current.GetInstance<IObjectStorageHelper>(ServiceLocatorConstants.LocalObjectStorageHelper);

            RegisterBackgroundTasks();
            InitializeHamburgerNavigationService();
        }

        #endregion

        #region Private methods

        private void RegisterBackgroundTasks()
        {
            // Remove old Background Tasks
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered("NewEpisodesBackgroundTask"))
            {
                BackgroundTaskHelper.Unregister("NewEpisodesBackgroundTask");
            }

            // Check if Background Task is already registered and register new ones
            if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(typeof(NewEpisodesV2BackgroundTask)))
            {
                BackgroundTaskHelper.Register(
                    nameof(NewEpisodesV2BackgroundTask),
                    new TimeTrigger(15, false),
                    conditions: new SystemCondition(SystemConditionType.InternetAvailable)
                );
            }
        }

        private void InitializeHamburgerNavigationService()
        {
            var menuItems = _hamburgerMenuService.MenuItems.Where(item => item.Type == MenuItemType.Main);
            var optionMenuItems = _hamburgerMenuService.MenuItems.Where(item => item.Type == MenuItemType.Options);

            foreach (var menuItem in menuItems)
            {
                MenuItems.Add(menuItem);
            }
            foreach (var optionMenuItem in optionMenuItems)
            {
                OptionMenuItems.Add(optionMenuItem);
            }

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

        #region Public methods

        public void HandleActivation()
        {
            // Retrieve activation values (for episode)
            var episodeId = _localObjectStorageHelper.Read<long?>(LocalStorageConstants.NavigateToEpisode);
            if (episodeId.HasValue && episodeId.Value > 0)
            {
                ServiceLocator.Current.GetInstance<EpisodeViewModel>().LoadEpisode(episodeId.Value);
                _hamburgerMenuService.NavigateTo(ViewConstants.Episode);
                _localObjectStorageHelper.Save(LocalStorageConstants.NavigateToEpisode, (long?)null);
            }
        }

        #endregion
    }
}
