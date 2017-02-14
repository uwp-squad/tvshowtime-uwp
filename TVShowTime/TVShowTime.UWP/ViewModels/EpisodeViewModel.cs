using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Model.Requests;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class EpisodeViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IEventService _eventService;

        #endregion

        #region Properties

        public Episode Episode { get; set; }

        #endregion

        #region Commands

        public ICommand ToggleWatchCommand { get; }
        public ICommand GoToPreviousEpisodeCommand { get; }
        public ICommand GoToNextEpisodeCommand { get; }

        #endregion

        #region Constructor

        public EpisodeViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;

            ToggleWatchCommand = new RelayCommand(ToggleWatch);
            GoToPreviousEpisodeCommand = new RelayCommand(GoToPreviousEpisode);
            GoToNextEpisodeCommand = new RelayCommand(GoToNextEpisode);
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

            toggleWatchObservable.Subscribe(async (episodeResponse) =>
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

        #endregion

        #region Private Methods

        private void RefreshByEpisodeId(long episodeId)
        {
            var request = new EpisodeRequestByEpisodeId { EpisodeId = episodeId };

            _tvshowtimeApiService.GetEpisode(request)
                .Subscribe(async (episodeResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        Episode = episodeResponse.Episode;
                        RaisePropertyChanged(nameof(Episode));
                    });
                },
                (error) =>
                {
                    throw new Exception();
                });
        }

        #endregion
    }
}
