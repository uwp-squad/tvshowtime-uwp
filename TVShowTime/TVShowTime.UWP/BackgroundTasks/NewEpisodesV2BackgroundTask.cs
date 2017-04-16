using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVShowTime.UWP.Constants;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;
using Windows.ApplicationModel.Background;
using Windows.Security.Credentials;
using Windows.UI.Notifications;

namespace TVShowTime.UWP.BackgroundTasks
{
    public class NewEpisodesV2BackgroundTask : ISingleProcessBackgroundTask
    {
        #region Public methods

        public async Task RunAsync(IBackgroundTaskInstance taskInstance)
        {
            // Create services
            var localObjectStorageHelper = new LocalObjectStorageHelper();

            // Check if notifications are enabled
            var notificationsEnabled = localObjectStorageHelper.Read(LocalStorageConstants.EnableNewEpisodeNotificationsOption, true);
            if (!notificationsEnabled)
            {
                return;
            }

            // Retrieve token to access API
            string token = RetrieveToken();

            // Create services (2)
            var tvshowtimeApiService = new TVShowTimeApiService(token);

            // Retrieve episodes from the agenda (episodes that will be aired soon)
            var agendaResponse = await tvshowtimeApiService.GetAgendaAsync();
            if (agendaResponse.Result == "OK")
            {
                // Retrieve list of episodes already notified
                var newEpisodesIdsNotified = new List<long>();
                if (await localObjectStorageHelper.FileExistsAsync(LocalStorageConstants.NewEpisodesIdsNotified))
                {
                    newEpisodesIdsNotified = await localObjectStorageHelper
                        .ReadFileAsync(LocalStorageConstants.NewEpisodesIdsNotified, new List<long>());
                }

                var episodesInAgenda = agendaResponse.Episodes;
                foreach (var episode in episodesInAgenda)
                {
                    var timeSpanDiff = episode.AirDate.Value.Subtract(DateTime.Now.ToUniversalTime());
                    if (episode.AirTime.HasValue)
                    {
                        timeSpanDiff = timeSpanDiff
                            .Add(TimeSpan.FromHours(episode.AirTime.Value.DateTime.Hour));
                        timeSpanDiff = timeSpanDiff
                            .Add(TimeSpan.FromMinutes(episode.AirTime.Value.DateTime.Minute));
                    }

                    if (newEpisodesIdsNotified.All(id => episode.Id != id) &&
                        timeSpanDiff.TotalDays <= 0)
                    {
                        // Create Toast notification when a new episode is out
                        GenerateToastNotification(episode);
                        newEpisodesIdsNotified.Add(episode.Id);
                    }
                }

                // Save the updated list in local storage
                await localObjectStorageHelper.SaveFileAsync(LocalStorageConstants.NewEpisodesIdsNotified, newEpisodesIdsNotified);
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

            // Create toast notification
            var toastNotification = new ToastNotification(toastContent.GetXml())
            {
                Group = episode.Show.Id.ToString()
            };

            // Show toast notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        #endregion
    }
}
