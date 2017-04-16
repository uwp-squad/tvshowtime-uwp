using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System.Windows.Input;
using TVShowTime.UWP.Constants;

namespace TVShowTime.UWP.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields

        private IObjectStorageHelper _localObjectStorageHelper;

        #endregion

        #region Properties

        private bool _enableNewEpisodeNotifications;
        public bool EnableNewEpisodeNotifications
        {
            get { return _enableNewEpisodeNotifications; }
            private set { _enableNewEpisodeNotifications = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand ToggleOptionCommand { get; }

        #endregion

        #region Constructor

        public SettingsViewModel()
        {
            _localObjectStorageHelper = ServiceLocator.Current.GetInstance<IObjectStorageHelper>(ServiceLocatorConstants.LocalObjectStorageHelper);

            EnableNewEpisodeNotifications = _localObjectStorageHelper.Read(LocalStorageConstants.EnableNewEpisodeNotificationsOption, true);

            ToggleOptionCommand = new RelayCommand<string>(ToggleOption);
        }

        #endregion

        #region Command Methods

        private void ToggleOption(string optionName)
        {
            if (nameof(EnableNewEpisodeNotifications) == optionName)
            {
                _localObjectStorageHelper.Save(LocalStorageConstants.EnableNewEpisodeNotificationsOption, !EnableNewEpisodeNotifications);
            }
        }

        #endregion
    }
}
