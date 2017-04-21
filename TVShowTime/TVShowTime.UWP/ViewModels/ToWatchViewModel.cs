using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class ToWatchViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IEventService _eventService;

        #endregion

        #region Properties

        public ObservableCollection<Episode> Episodes { get; } = new ObservableCollection<Episode>();

        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand SelectEpisodeCommand { get; private set; }

        #endregion

        #region Constructor

        public ToWatchViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IHamburgerMenuService hamburgerMenuService,
            IEventService eventService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _hamburgerMenuService = hamburgerMenuService;
            _eventService = eventService;

            SelectEpisodeCommand = new RelayCommand<Episode>(SelectEpisode);

            Refresh();

            _eventService.WatchEpisodeEvent
                .Subscribe((episode) =>
                {
                    Refresh();
                });

            _eventService.UnwatchEpisodeEvent
               .Subscribe((episode) =>
               {
                   Refresh();
               });

            _eventService.FollowShowEvent
                .Subscribe((show) =>
                {
                    Refresh();
                });

            _eventService.UnfollowShowEvent
                .Subscribe((show) =>
                {
                    var episodeOfTheShow = Episodes.FirstOrDefault(e => e.Show.Id == show.Id);
                    if (episodeOfTheShow != null)
                    {
                        Episodes.Remove(episodeOfTheShow);
                    }
                });
        }

        #endregion

        #region Command Methods

        public void SelectEpisode(Episode episode)
        {
            ServiceLocator.Current.GetInstance<EpisodeViewModel>().SelectEpisode(episode);
            _hamburgerMenuService.NavigateTo(ViewConstants.Episode);
        }

        #endregion

        #region Private Methods

        private void Refresh()
        {
            IsLoading = true;

            _tvshowtimeApiService.GetWatchlist(0, 0)
                .Subscribe(async (watchlistResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        Episodes.Clear();

                        foreach (var episode in watchlistResponse.Episodes)
                        {
                            Episodes.Add(episode);
                        }

                        IsLoading = false;
                    });
                },
                async (error) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        IsLoading = false;
                    });
                    throw new Exception();
                });
        }

        #endregion
    }
}
