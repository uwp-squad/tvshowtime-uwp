using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TVShowTime.UWP.Configuration;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Models;
using TVShowTimeApi.Services;
using Windows.Security.Credentials;

namespace TVShowTime.UWP.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private INavigationService _navigationService;

        private IObjectStorageHelper _localObjectStorageHelper;

        #endregion

        #region Properties

        private bool _loginFailed;
        public bool LoginFailed
        {
            get { return _loginFailed; }
            private set { _loginFailed = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand TryAuthenticateCommand { get; }

        #endregion

        #region Constructor

        public LoginViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            INavigationService navigationService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _navigationService = navigationService;

            _localObjectStorageHelper = ServiceLocator.Current.GetInstance<IObjectStorageHelper>(ServiceLocatorConstants.LocalObjectStorageHelper);

            TryAuthenticateCommand = new RelayCommand(TryAuthenticate);
        }

        #endregion

        #region Command Methods

        public async void TryAuthenticate()
        {
            if (LoginFailed)
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    LoginFailed = false;
                });
            }

            // Do not login if we are already connected (token already set)
            // Retrieve token and username from previous connections
            string username = RetrieveUserTokenAndUsername();

            if (!string.IsNullOrWhiteSpace(_tvshowtimeApiService.Token) && !string.IsNullOrWhiteSpace(username))
            {
                // Check if token has expired
                var userConnectionProfile = await RetrieveConnectionProfileByUsernameAsync(username);
                if (userConnectionProfile != null && userConnectionProfile.ExpirationDate > DateTime.Now)
                {
                    _navigationService.NavigateTo(ViewConstants.Main);
                    return;
                }
            }

            // Login if there is no current user already logged
            _tvshowtimeApiService.Login(AuthConstants.ClientId, AuthConstants.ClientSecret)
                .Subscribe(async (result) =>
                {
                    if (result.HasValue && result.Value)
                    {
                        HandleLoginSuccess();
                    }
                    else
                    {
                        await HandleLoginFailedAsync();
                    }
                },
                async (error) =>
                {
                    await HandleLoginFailedAsync();
                });
        }

        #endregion

        #region Private Methods

        private string RetrieveUserTokenAndUsername()
        {
            string username = null;

            try
            {
                PasswordCredential credential = null;
                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource(LoginConstants.AppResource);

                if (credentialList.Count > 0)
                {
                    // When there are multiple usernames, retrieve the first user profile saved
                    credential = credentialList[0];
                }

                credential.RetrievePassword();
                _tvshowtimeApiService.Token = credential.Password;
                username = credential.UserName;
            }
            catch
            {
                _tvshowtimeApiService.Token = null;
            }

            return username;
        }

        private void SaveUserToken(string username)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(LoginConstants.AppResource, username, _tvshowtimeApiService.Token));
        }

        private async Task HandleLoginFailedAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                LoginFailed = true;
            });
        }

        private void HandleLoginSuccess()
        {
            _tvshowtimeApiService.GetCurrentUser().Subscribe(async (userResponse) =>
            {
                // Cache access token in Password vault
                SaveUserToken(userResponse.User.Username);

                // Save user information (connection profile) in local storage
                var userConnectionProfile = new UserConnnectionProfile
                {
                    User = userResponse.User,
                    ExpirationDate = DateTime.Now.AddSeconds(LoginConstants.TokenLifetime)
                };
                await AddUserConnectionProfileAsync(userConnectionProfile);

                // Navigate to main page
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    _navigationService.NavigateTo(ViewConstants.Main);
                });
            });
        }

        #endregion

        #region Manage User Connection Profiles

        private async Task<Dictionary<string, UserConnnectionProfile>> RetrieveDictionaryOfConnectionProfilesAsync()
        {
            var dictionaryOfUserConnectionProfiles = new Dictionary<string, UserConnnectionProfile>();
            if (await _localObjectStorageHelper.FileExistsAsync(LocalStorageConstants.UserConnectionProfiles))
            {
                dictionaryOfUserConnectionProfiles = await _localObjectStorageHelper
                    .ReadFileAsync(LocalStorageConstants.UserConnectionProfiles, dictionaryOfUserConnectionProfiles);
            }

            return dictionaryOfUserConnectionProfiles;
        }

        private async Task<UserConnnectionProfile> RetrieveConnectionProfileByUsernameAsync(string username)
        {
            // Retrieve a connection profile by username
            var dictionaryOfUserConnectionProfiles = await RetrieveDictionaryOfConnectionProfilesAsync();

            if (!dictionaryOfUserConnectionProfiles.ContainsKey(username))
                return null;

            return dictionaryOfUserConnectionProfiles[username];
        }

        private async Task AddUserConnectionProfileAsync(UserConnnectionProfile userConnectionProfile)
        {
            // Set the connection profiles inside the list of all existing profiles
            var dictionaryOfUserConnectionProfiles = await RetrieveDictionaryOfConnectionProfilesAsync();
            dictionaryOfUserConnectionProfiles[userConnectionProfile.User.Username] = userConnectionProfile;

            // Save the dictionary in local storage
            await _localObjectStorageHelper.SaveFileAsync(LocalStorageConstants.UserConnectionProfiles, dictionaryOfUserConnectionProfiles);
        }

        #endregion
    }
}