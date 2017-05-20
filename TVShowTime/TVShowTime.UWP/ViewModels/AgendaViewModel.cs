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
    public class AgendaViewModel : ViewModelBase, IRefreshable, ILoadable
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IEventService _eventService;
        private IToastNotificationService _toastNotificationService;

        private int _currentReversePage = 0;
        private int _currentPage = 0;

        #endregion

        #region Properties

        public ObservableCollection<AgendaGroup> Groups { get; } = new ObservableCollection<AgendaGroup>();

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
                return !IsLoading;
            }
        }

        public bool ShouldRefresh
        {
            get
            {
                // TODO : Listen if a show has been followed/unfollowed from the collection
                return DateTime.Now.Day != LastLoadingDate.Day ||
                    DateTime.Now.Month != LastLoadingDate.Month ||
                    DateTime.Now.Year != LastLoadingDate.Year;
            }
        }

        #endregion

        #region Commands

        public ICommand SelectEpisodeCommand { get; private set; }

        #endregion

        #region Constructor

        public AgendaViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IHamburgerMenuService hamburgerMenuService,
            IEventService eventService,
            IToastNotificationService toastNotificationService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _hamburgerMenuService = hamburgerMenuService;
            _eventService = eventService;
            _toastNotificationService = toastNotificationService;

            SelectEpisodeCommand = new RelayCommand<Episode>(SelectEpisode);

            Refresh();
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
            if (!IsLoading)
            {
                _currentReversePage--;
                LoadAgendaPage(_currentReversePage);
            }
        }

        public void LoadNextPage()
        {
            if (!IsLoading)
            {
                _currentPage++;
                LoadAgendaPage(_currentPage);
            }
        }

        public void Refresh()
        {
            Groups.Clear();

            LastLoadingDate = DateTime.Now;
            _currentReversePage = 0;
            _currentPage = 0;

            LoadAgendaPage(_currentPage);
        }

        #endregion

        #region Private Methods

        private void LoadAgendaPage(int page)
        {
            IsLoading = true;

            _tvshowtimeApiService.GetAgenda(page, 10, true)
                .Subscribe(async (agendaResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        foreach (var episode in agendaResponse.Episodes)
                        {
                            // Do not add an episode without air date
                            if (!episode.AirDate.HasValue)
                                continue;

                            // Do not add the same episode twice
                            if (IsAlreadyAdded(episode))
                                continue;

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

        private bool IsAlreadyAdded(Episode episode)
        {
            return Groups.Any(g => g.Episodes.Any(e => e.Id == episode.Id));
        }

        #endregion
    }
}
