using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Windows.Input;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Infrastructure;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Model.Requests;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class EpisodeViewModel : ViewModelBase, IRefreshable, ILoadable
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IEventService _eventService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IToastNotificationService _toastNotificationService;

        #endregion

        #region Properties

        public Episode Episode { get; set; }

        public Emotion GoodEmotion { get; } = Emotion.Good;
        public Emotion FunEmotion { get; } = Emotion.Fun;
        public Emotion WowEmotion { get; } = Emotion.Wow;
        public Emotion SadEmotion { get; } = Emotion.Sad;
        public Emotion SosoEmotion { get; } = Emotion.Soso;
        public Emotion BadEmotion { get; } = Emotion.Bad;

        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            private set { _isLoading = value; RaisePropertyChanged(); }
        }

        public DateTime LastLoadingDate { get; private set; }

        public bool CanRefresh
        {
            get
            {
                return !IsLoading && Episode != null && Episode.Id > 0;
            }
        }

        public bool ShouldRefresh
        {
            get
            {
                // TODO : Listen if an episode is watched/unwatched outside this ViewModel
                return DateTime.Now.Subtract(LastLoadingDate).TotalHours > 1;
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleWatchCommand { get; }
        public ICommand GoToPreviousEpisodeCommand { get; }
        public ICommand GoToNextEpisodeCommand { get; }
        public ICommand ToggleEmotionCommand { get; }
        public ICommand SelectShowCommand { get; }

        #endregion

        #region Constructor

        public EpisodeViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService,
            IHamburgerMenuService hamburgerMenuService,
            IToastNotificationService toastNotificationService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;
            _hamburgerMenuService = hamburgerMenuService;
            _toastNotificationService = toastNotificationService;

            ToggleWatchCommand = new RelayCommand(ToggleWatch);
            GoToPreviousEpisodeCommand = new RelayCommand(GoToPreviousEpisode);
            GoToNextEpisodeCommand = new RelayCommand(GoToNextEpisode);
            ToggleEmotionCommand = new RelayCommand<Emotion>(ToggleEmotion);
            SelectShowCommand = new RelayCommand<Episode>(SelectShow);
        }

        #endregion

        #region Command Methods

        private void ToggleWatch()
        {
            bool markAsWatch = !(Episode.Seen.HasValue && Episode.Seen.Value);

            var request = new EpisodeRequestByEpisodeId { EpisodeId = Episode.Id };
            IObservable<Response> toggleWatchObservable;

            if (markAsWatch)
                toggleWatchObservable = _tvshowtimeApiService.MarkEpisodeWatched(request, false, false);
            else
                toggleWatchObservable = _tvshowtimeApiService.UnmarkEpisodeWatched(request);

            toggleWatchObservable.Subscribe(async (response) =>
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    Episode.Seen = !Episode.Seen;
                    RaisePropertyChanged(nameof(Episode));

                    if (markAsWatch)
                        _eventService.WatchEpisode(Episode);
                    else
                        _eventService.UnwatchEpisode(Episode);
                });
            },
            (error) =>
            {
                throw new Exception();
            });
        }

        private void GoToPreviousEpisode()
        {
            RefreshByEpisodeId(Episode.PreviousEpisode.Id);
        }

        private void GoToNextEpisode()
        {
            RefreshByEpisodeId(Episode.NextEpisode.Id);
        }

        private void ToggleEmotion(Emotion emotion)
        {
            if (Episode.Emotion == null || Episode.Emotion.Id != (int)emotion)
            {
                // Set emotion selected
                _tvshowtimeApiService.SetEmotionForEpisode(Episode.Id, emotion)
                    .Subscribe(async (response) =>
                    {
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            Episode.Emotion = new EpisodeEmotion
                            {
                                Id = (int)emotion,
                                Name = emotion.ToString()
                            };
                            RaisePropertyChanged(nameof(Episode));
                        });
                    },
                    (error) =>
                    {
                        throw new Exception();
                    });
            }
            else
            {
                // Remove emotion
                _tvshowtimeApiService.DeleteEmotionForEpisode(Episode.Id)
                    .Subscribe(async (response) =>
                    {
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            Episode.Emotion = null;
                            RaisePropertyChanged(nameof(Episode));
                        });
                    },
                    (error) =>
                    {
                        throw new Exception();
                    });
            }
        }

        private void SelectShow(Episode episode)
        {
            ServiceLocator.Current.GetInstance<ShowViewModel>().LoadShow(episode.Show.Id);
            _hamburgerMenuService.NavigateTo(ViewConstants.Show);
        }

        #endregion

        #region Public Methods

        public void SelectEpisode(Episode episode)
        {
            RefreshByEpisodeId(episode.Id);
        }

        public void LoadEpisode(long episodeId)
        {
            RefreshByEpisodeId(episodeId);
        }

        public void Refresh()
        {
            RefreshByEpisodeId(Episode.Id);
        }

        #endregion

        #region Private Methods

        private void RefreshByEpisodeId(long episodeId)
        {
            IsLoading = true;

            _tvshowtimeApiService.GetEpisode(new EpisodeRequestByEpisodeId { EpisodeId = episodeId })
                .Subscribe(async (episodeResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        LastLoadingDate = DateTime.Now;

                        Episode = episodeResponse.Episode;
                        RaisePropertyChanged(nameof(Episode));

                        IsLoading = false;
                    });
                },
                async (error) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        IsLoading = false;
                    });

                    _toastNotificationService.ShowErrorNotification("An error happened. Please retry later.");
                });
        }

        #endregion
    }
}
