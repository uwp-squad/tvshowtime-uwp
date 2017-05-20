using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Infrastructure;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class ToWatchViewModel : ViewModelBase, IRefreshable, ILoadable
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IEventService _eventService;

        private bool _watchedOrUnwatchedEpisode = false;
        private bool _followedOrUnfollowedShow = false;

        #endregion

        #region Properties

        public ObservableCollection<Episode> Episodes { get; } = new ObservableCollection<Episode>();

        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; RaisePropertyChanged(); }
        }

        public DateTime LastLoadingDate { get; private set; }

        public bool CanRefresh
        {
            get
            {
                return !IsLoading;
            }
        }

        public bool ShouldRefresh
        {
            get
            {
                return _watchedOrUnwatchedEpisode || _followedOrUnfollowedShow || DateTime.Now.Subtract(LastLoadingDate).TotalHours > 1;
            }
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
                .Concat(_eventService.UnwatchEpisodeEvent)
                .Subscribe((episode) =>
                {
                    _watchedOrUnwatchedEpisode = true;
                });

            _eventService.FollowShowEvent
                .Concat(_eventService.UnfollowShowEvent)
                .Subscribe((show) =>
                {
                    _followedOrUnfollowedShow = true;
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

        #region Public Methods

        public void Refresh()
        {
            IsLoading = true;

            _tvshowtimeApiService.GetWatchlist(0, 0)
                .Subscribe(async (watchlistResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        _watchedOrUnwatchedEpisode = false;
                        _followedOrUnfollowedShow = false;
                        LastLoadingDate = DateTime.Now;

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
