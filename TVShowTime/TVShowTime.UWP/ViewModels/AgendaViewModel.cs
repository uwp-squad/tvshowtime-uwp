using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Models;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class AgendaViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IEventService _eventService;

        private int _currentReversePage = 0;
        private int _currentPage = 0;
        private bool _isLoadingAgenda = false;

        #endregion

        #region Properties

        public ObservableCollection<AgendaGroup> Groups { get; } = new ObservableCollection<AgendaGroup>();

        #endregion

        #region Commands

        public ICommand SelectEpisodeCommand { get; private set; }

        #endregion

        #region Constructor

        public AgendaViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IHamburgerMenuService hamburgerMenuService,
            IEventService eventService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _hamburgerMenuService = hamburgerMenuService;
            _eventService = eventService;

            SelectEpisodeCommand = new RelayCommand<Episode>(SelectEpisode);

            LoadAgendaPage(_currentPage);
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

        public void LoadPreviousPage()
        {
            if (!_isLoadingAgenda)
            {
                _currentReversePage--;
                LoadAgendaPage(_currentReversePage);
            }
        }

        public void LoadNextPage()
        {
            if (!_isLoadingAgenda)
            {
                _currentPage++;
                LoadAgendaPage(_currentPage);
            }
        }

        #endregion

        #region Private Methods

        private void LoadAgendaPage(int page)
        {
            _isLoadingAgenda = true;

            _tvshowtimeApiService.GetAgenda(page, 10, true)
                .Subscribe(async (agendaResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        foreach (var episode in agendaResponse.Episodes)
                        {
                            // Do not add an episode without air date
                            if (!episode.AirDate.HasValue)
                                return;

                            // Do not add the same episode twice
                            if (IsAlreadyAdded(episode))
                                return;

                            // Add episode to the corresponding group
                            var group = Groups.FirstOrDefault(g => g.Date == episode.AirDate);

                            // Create a new group if necessary
                            if (group == null)
                            {
                                group = new AgendaGroup { Date = episode.AirDate.Value };
                                int index = Groups.Count(g => g.Date < episode.AirDate.Value);

                                Groups.Insert(index, group);
                            }

                            group.Episodes.Add(episode);
                        }

                        _isLoadingAgenda = false;
                    });
                },
                (error) =>
                {
                    throw new Exception();
                });
        }

        private bool IsAlreadyAdded(Episode episode)
        {
            return Groups.Any(g => g.Episodes.Any(e => e.Id == episode.Id));
        }

        #endregion
    }
}
