using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Infrastructure;
using TVShowTime.UWP.Models;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class ShowViewModel : ViewModelBase, IRefreshable, ILoadable
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IEventService _eventService;
        private IToastNotificationService _toastNotificationService;
        private IHamburgerMenuService _hamburgerMenuService;

        #endregion

        #region Properties

        public Show Show { get; private set; }

        public ObservableCollection<ShowSeasonGroup> Seasons { get; } = new ObservableCollection<ShowSeasonGroup>();

        private ShowSeasonGroup _selectedSeason;
        public ShowSeasonGroup SelectedSeason
        {
            get { return _selectedSeason; }
            private set { _selectedSeason = value; RaisePropertyChanged(); }
        }

        public int MinSeasonNumber
        {
            get
            {
                if (Seasons == null || Seasons.Count <= 0)
                    return 0;
                return Seasons.Min(s => s.SeasonNumber);
            }
        }

        public int MaxSeasonNumber
        {
            get
            {
                if (Seasons == null || Seasons.Count <= 0)
                    return 0;
                return Seasons.Max(s => s.SeasonNumber);
            }
        }

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
                return !IsLoading && Show != null && Show.Id > 0;
            }
        }

        public bool ShouldRefresh
        {
            get
            {
                // TODO : Listen if an episode is watched/unwatched outside this ViewModel
                return DateTime.Now.Subtract(LastLoadingDate).TotalHours > 3;
            }
        }

        #endregion

        #region Commands

        public ICommand GoToPreviousSeasonCommand { get; }
        public ICommand GoToNextSeasonCommand { get; }
        public ICommand SelectEpisodeCommand { get; }

        #endregion

        #region Constructor

        public ShowViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService,
            IToastNotificationService toastNotificationService,
            IHamburgerMenuService hamburgerMenuService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;
            _toastNotificationService = toastNotificationService;
            _hamburgerMenuService = hamburgerMenuService;

            GoToPreviousSeasonCommand = new RelayCommand(GoToPreviousSeason);
            GoToNextSeasonCommand = new RelayCommand(GoToNextSeason);
            SelectEpisodeCommand = new RelayCommand<Episode>(SelectEpisode);
        }

        #endregion

        #region Command Methods

        private void GoToPreviousSeason()
        {
            SelectedSeason = Seasons.FirstOrDefault(s => s.SeasonNumber == SelectedSeason.SeasonNumber - 1);
        }

        private void GoToNextSeason()
        {
            SelectedSeason = Seasons.FirstOrDefault(s => s.SeasonNumber == SelectedSeason.SeasonNumber + 1);
        }

        private void SelectEpisode(Episode episode)
        {
            ServiceLocator.Current.GetInstance<EpisodeViewModel>().SelectEpisode(episode);
            _hamburgerMenuService.NavigateTo(ViewConstants.Episode);
        }

        #endregion

        #region Public Methods

        public void LoadShow(long showId, int selectedSeason = 1)
        {
            RefreshByShowId(showId, selectedSeason);
        }

        public void Refresh()
        {
            RefreshByShowId(Show.Id, SelectedSeason?.SeasonNumber ?? 1);
        }

        #endregion

        #region Private Methods

        private void RefreshByShowId(long showId, int selectedSeason = 1)
        {
            IsLoading = true;

            _tvshowtimeApiService.GetShow(showId, string.Empty, true)
                .Subscribe(async (showResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        LastLoadingDate = DateTime.Now;

                        Seasons.Clear();

                        Show = showResponse.Show;
                        RaisePropertyChanged(nameof(Show));

                        foreach (var episode in Show.Episodes)
                        {
                            var seasonGroup = Seasons.FirstOrDefault(s => s.SeasonNumber == episode.Season);
                            if (seasonGroup == null)
                            {
                                seasonGroup = new ShowSeasonGroup { SeasonNumber = episode.Season };
                                Seasons.Add(seasonGroup);
                            }

                            seasonGroup.Episodes.Add(episode);
                        }

                        RaisePropertyChanged(nameof(MinSeasonNumber));
                        RaisePropertyChanged(nameof(MaxSeasonNumber));

                        SelectedSeason = Seasons.FirstOrDefault(s => s.SeasonNumber == selectedSeason);

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
