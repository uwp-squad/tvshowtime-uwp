using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTime.UWP.Constants;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;
using Windows.ApplicationModel.Background;
using Windows.Security.Credentials;
using Windows.UI.Notifications;

namespace TVShowTime.UWP.BackgroundTasks
{
    public class NewEpisodesBackgroundTask : IBackgroundTask
    {
        #region Public methods

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Retrieve token to access API
            string token = RetrieveToken();

            // Create services
            var localObjectStorageHelper = new LocalObjectStorageHelper();
            var tvshowtimeApiService = new TVShowTimeApiService(token);

            // Retrieve episodes from the agenda (episodes that will be aired soon)
            var agendaResponse = await tvshowtimeApiService.GetAgendaAsync();
            if (agendaResponse.Result == "OK")
            {
                // Retrieve list of episodes already selected
                var newEpisodesIdsSelected = new List<long>();

                if (await localObjectStorageHelper.FileExistsAsync(LocalStorageConstants.NewEpisodesIdsSelected))
                {
                    await localObjectStorageHelper.ReadFileAsync(LocalStorageConstants.NewEpisodesIdsSelected, new List<long>());
                }

                var episodesInAgenda = agendaResponse.Episodes;
                foreach (var episode in episodesInAgenda)
                {
                    if (episode.AirDate.HasValue && episode.AirDate > DateTime.Now && newEpisodesIdsSelected.All(id => episode.Id != id))
                    {
                        // Create Toast notification when a new episode is out
                        GenerateToastNotification(episode);
                        newEpisodesIdsSelected.Add(episode.Id);
                    }
                }

                // Save the updated list in local storage
                await localObjectStorageHelper.SaveFileAsync(LocalStorageConstants.NewEpisodesIdsSelected, newEpisodesIdsSelected);
            }
        }

        #endregion

        #region Private methods

        private string RetrieveToken()
        {
            try
            {
                PasswordCredential credential = null;
                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource(LoginConstants.AppResource);

                if (credentialList.Count > 0)
                {
                    // When there are multiple usernames, retrieve the first credential
                    credential = credentialList[0];
                }

                credential.RetrievePassword();
                return credential.Password;
            }
            catch
            {
                return null;
            }
        }

        private void GenerateToastNotification(Episode episode)
        {
            // Create content of the toast notification
            var toastContent = new ToastContent()
            {
                Launch = $"action={NotificationConstants.AlertNewEpisode}&episodeId={episode.Id}",
                Scenario = ToastScenario.Default,
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = $"A new episode of {episode.Show.Name} is out."
                            }
                        },
                        AppLogoOverride = new ToastGenericAppLogo
                        {
                            Source = episode.Show.Images.Posters.One
                        }
                    }
                }
            };

            // Add Hero image if it is not a default image
            if (!episode.Images.Screen.One.Contains("default"))
            {
                toastContent.Visual.BindingGeneric.HeroImage = new ToastGenericHeroImage
                {
                    Source = episode.Images.Screen.One
                };
            }

            // Create toast notification at due date
            DateTime dueDateTime = episode.AirDate.Value;
            var toastNotification = new ScheduledToastNotification(toastContent.GetXml(), dueDateTime)
            {
                Group = episode.Show.Id.ToString()
            };

            // Schedule toast notification
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toastNotification);
        }

        #endregion
    }
}
