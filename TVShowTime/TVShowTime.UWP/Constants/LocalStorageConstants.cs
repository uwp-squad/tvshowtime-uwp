using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Key of the list of episodes ids selected in <see cref="NewEpisodesBackgroundTask"/> saved in local storage
        /// </summary>
        public const string NewEpisodesIdsSelected = "newEpisodesIdsSelected.txt";

        /// <summary>
        /// Key of the item (EpisodeId) that will be used to navigate to <see cref="EpisodePage"/>
        /// </summary>
        public const string NavigateToEpisode = "navigateToEpisode";
    }
}
