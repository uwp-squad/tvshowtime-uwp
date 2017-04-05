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
    public class UpcomingViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IHamburgerMenuService _hamburgerMenuService;

        private const int _pageSize = 15;
        private int _currentPage = 0;

        #endregion

        #region Properties

        public ObservableCollection<UpcomingEpisodeViewModel> Episodes { get; } = new ObservableCollection<UpcomingEpisodeViewModel>();

        #endregion

        #region Commands

        public ICommand SelectEpisodeCommand { get; private set; }

        #endregion

        #region Constructor

        public UpcomingViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IHamburgerMenuService hamburgerMenuService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _hamburgerMenuService = hamburgerMenuService;

            SelectEpisodeCommand = new RelayCommand<UpcomingEpisodeViewModel>(SelectEpisode);

            Initialize();
        }

        #endregion

        #region Command Methods

        public void SelectEpisode(UpcomingEpisodeViewModel episode)
        {
            ServiceLocator.Current.GetInstance<EpisodeViewModel>().LoadEpisode(episode.Id);
            _hamburgerMenuService.NavigateTo(ViewConstants.Episode);
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            _currentPage = 0;
            LoadUpcomingEpisodes();
        }

        private void LoadUpcomingEpisodes()
        {
            _tvshowtimeApiService.GetAgenda(_currentPage, _pageSize)
                .Subscribe(async (agendaResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        foreach (var episode in agendaResponse.Episodes)
                        {
                            // Do not add the same show twice
                            if (Episodes.Any(e => e.Id == episode.Id))
                                continue;

                            // Do not add episode if already aired
                            if (episode.AirDate < DateTime.Now)
                                continue;
                            
                            string diffTime = string.Empty;
                            var timeSpanDiff = episode.AirDate.Value.Subtract(DateTime.Now.ToUniversalTime());
                            if (episode.AirTime.HasValue)
                            {
                                timeSpanDiff = timeSpanDiff
                                    .Add(TimeSpan.FromHours(episode.AirTime.Value.DateTime.Hour));
                                timeSpanDiff = timeSpanDiff
                                    .Add(TimeSpan.FromMinutes(episode.AirTime.Value.DateTime.Minute));
                            }

                            if (timeSpanDiff.Days >= 7)
                            {
                                diffTime += $"{timeSpanDiff.Days} days";
                            }
                            else
                            {
                                if (timeSpanDiff.Days >= 1)
                                {
                                    diffTime += $"{timeSpanDiff.Days} day";
                                    if (timeSpanDiff.Days > 1)
                                        diffTime += "s";
                                }

                                if (timeSpanDiff.Hours >= 1)
                                {
                                    if (!string.IsNullOrWhiteSpace(diffTime))
                                        diffTime += Environment.NewLine;

                                    diffTime += $"{timeSpanDiff.Hours} hour";
                                    if (timeSpanDiff.Hours > 1)
                                        diffTime += "s";
                                }

                                if (timeSpanDiff.Minutes >= 1)
                                {
                                    if (!string.IsNullOrWhiteSpace(diffTime))
                                        diffTime += Environment.NewLine;

                                    diffTime += $"{timeSpanDiff.Minutes} min.";
                                }
                            }

                            Episodes.Add(new UpcomingEpisodeViewModel
                            {
                                Id = episode.Id,
                                Season = episode.Season,
                                Number = episode.Number,
                                Show = episode.Show,
                                DiffTime = diffTime.Trim(),
                                Original = episode
                            });
                        }

                        if (agendaResponse.Episodes.Count >= _pageSize)
                        {
                            _currentPage++;
                            LoadUpcomingEpisodes();
                        }
                    });
                },
                (error) =>
                {
                    throw new Exception();
                });
        }

        #endregion

        #region Private ViewModels

        public class UpcomingEpisodeViewModel : ViewModelBase
        {
            public long Id { get; set; }
            public int Season { get; set; }
            public int Number { get; set; }
            public Show Show { get; set; }

            private string _diffTime;
            public string DiffTime
            {
                get { return _diffTime; }
                set { _diffTime = value; RaisePropertyChanged(); }
            }

            public Episode Original { get; set; }
        }

        #endregion
    }
}
