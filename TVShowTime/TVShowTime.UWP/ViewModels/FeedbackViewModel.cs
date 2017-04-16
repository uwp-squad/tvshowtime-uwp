using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Services.Store.Engagement;
using System.Windows.Input;
using TVShowTime.UWP.Models;
using System.Collections.ObjectModel;
using Windows.System;

namespace TVShowTime.UWP.ViewModels
{
    public class FeedbackViewModel : ViewModelBase
    {
        #region Properties

        public ObservableCollection<FeedbackCard> Cards { get; } = new ObservableCollection<FeedbackCard>();

        #endregion

        #region Commands

        public ICommand LaunchFeedbackHubCommand { get; }
        public ICommand OpenFeedbackCardCommand { get; }

        #endregion

        #region Constructor

        public FeedbackViewModel()
        {
            Cards.Add(new FeedbackCard
            {
                Title = "Vote for the best character",
                Description = "Add ability to vote for the best character for an episode.",
                State = FeedbackCardState.NeedVotes,
                Link = "http://support.tvshowtime.com/forums/172228-general/suggestions/18766111-vote-for-the-best-character"
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Comments",
                Description = "Add ability to read and write comments for an episode.",
                State = FeedbackCardState.NeedVotes,
                Link = "http://support.tvshowtime.com/forums/172228-general/suggestions/18766129-comments"
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Ability to register",
                Description = "Add ability to register / sign up inside the app using a registering process.",
                State = FeedbackCardState.NeedVotes,
                Link = "http://support.tvshowtime.com/forums/172228-general/suggestions/18766147-ability-to-register"
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Upcoming episodes",
                Description = "Improve the performance and the quality data from the upcoming page.",
                State = FeedbackCardState.NeedVotes,
                Link = "http://support.tvshowtime.com/forums/172228-general/suggestions/18766096-create-api-endpoint-to-retrieve-upcoming-episodes"
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Search TV shows",
                Description = "Add ability to retrieve TV shows by their names.",
                State = FeedbackCardState.NeedVotes,
                Link = "http://support.tvshowtime.com/forums/172228-general/suggestions/18950398-search-tv-shows"
            });

            Cards.Add(new FeedbackCard
            {
                Title = "Show",
                Description = "See the complete description of a TV show (name, description, list of seasons and episodes, seen episodes, next episode).",
                State = FeedbackCardState.InProgress
            });

            Cards.Add(new FeedbackCard
            {
                Title = "Episode",
                Description = "See the complete description of an episode (name, description, emotion).",
                State = FeedbackCardState.Done
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Collection",
                Description = "See the collection of all your TV shows.",
                State = FeedbackCardState.Done
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Explore",
                Description = "List of TV shows that you can add to your watch list.",
                State = FeedbackCardState.Done
            });
            Cards.Add(new FeedbackCard
            {
                Title = "Agenda",
                Description = "Get an overview of the agenda of future and past episodes.",
                State = FeedbackCardState.Done
            });
            Cards.Add(new FeedbackCard
            {
                Title = "To watch",
                Description = "List of episodes to watch.",
                State = FeedbackCardState.Done
            });

            LaunchFeedbackHubCommand = new RelayCommand(LaunchFeedbackHub);
            OpenFeedbackCardCommand = new RelayCommand<FeedbackCard>(OpenFeedbackCard);
        }

        #endregion

        #region Command Methods

        private async void LaunchFeedbackHub()
        {
            if (StoreServicesFeedbackLauncher.IsSupported())
            {
                await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
            }
        }

        private async void OpenFeedbackCard(FeedbackCard card)
        {
            if (!string.IsNullOrWhiteSpace(card.Link))
            {
                await Launcher.LaunchUriAsync(new Uri(card.Link));
            }
        }

        #endregion
    }
}
