using TVShowTime.UWP.BackgroundTasks;
using TVShowTime.UWP.Views;

namespace TVShowTime.UWP.Constants
{
    public static class LocalStorageConstants
    {
        /// <summary>
        /// Key of the list of user connection profiles saved in local storage
        /// </summary>
        public const string UserConnectionProfiles = "userConnectionProfiles";
        
        /// <summary>
        /// Key of the list of episodes ids already used (notification already shown) in <see cref="NewEpisodesV2BackgroundTask"/> saved in local storage
        /// </summary>
        public const string NewEpisodesIdsNotified = "newEpisodesIdsNotified.txt";

        /// <summary>
        /// Key of the item (EpisodeId) that will be used to navigate to <see cref="EpisodePage"/>
        /// </summary>
        public const string NavigateToEpisode = "navigateToEpisode";

        /// <summary>
        /// Key of the option to enable new episode notifications <see cref="NewEpisodesV2BackgroundTask"/>
        /// </summary>
        public const string EnableNewEpisodeNotificationsOption = "enableNewEpisodeNotificationsOption";
    }
}
