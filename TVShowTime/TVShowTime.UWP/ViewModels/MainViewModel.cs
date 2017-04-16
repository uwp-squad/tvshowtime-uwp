using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using Microsoft.Toolkit.Uwp;
using Microsoft.Practices.ServiceLocation;
using TVShowTime.UWP.BackgroundTasks;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Services;
using Windows.ApplicationModel.Background;

namespace TVShowTime.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private INavigationService _navigationService;
        private IHamburgerMenuService _hamburgerMenuService;

        private IObjectStorageHelper _localObjectStorageHelper;

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
