﻿using GalaSoft.MvvmLight;
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
    public class CollectionViewModel : ViewModelBase, IRefreshable, ILoadable
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IEventService _eventService;
        private IHamburgerMenuService _hamburgerMenuService;
        private IToastNotificationService _toastNotificationService;

        private const int _pageSize = 50;
        private int _currentPage = 0;

        private ShowCollectionGroup _allGroup;
        private ShowCollectionGroup _lateGroup;
        private ShowCollectionGroup _upToDateGroup;
        private ShowCollectionGroup _continuingGroup;
        private ShowCollectionGroup _endedGroup;
        private ShowCollectionGroup _archivedGroup;

        #endregion

        #region Properties

        public ObservableCollection<ShowCollectionGroup> Groups { get; } = new ObservableCollection<ShowCollectionGroup>();

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
                return DateTime.Now.Subtract(LastLoadingDate).TotalHours > 6;
            }
        }

        #endregion

        #region Commands

        public ICommand SelectShowCommand { get; }

        #endregion

        #region Constructor

        public CollectionViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService,
            IHamburgerMenuService hamburgerMenuService,
            IToastNotificationService toastNotificationService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;
            _hamburgerMenuService = hamburgerMenuService;
            _toastNotificationService = toastNotificationService;

            SelectShowCommand = new RelayCommand<Show>(SelectShow);

            _allGroup = new ShowCollectionGroup { Name = "All" };
            _lateGroup = new ShowCollectionGroup { Name = "Late" };
            _upToDateGroup = new ShowCollectionGroup { Name = "Up to date" };
            _continuingGroup = new ShowCollectionGroup { Name = "Continuing" };
            _endedGroup = new ShowCollectionGroup { Name = "Ended" };
            _archivedGroup = new ShowCollectionGroup { Name = "Archived" };

            Groups.Add(_allGroup);
            Groups.Add(_lateGroup);
            Groups.Add(_upToDateGroup);
            Groups.Add(_continuingGroup);
            Groups.Add(_endedGroup);
            Groups.Add(_archivedGroup);

            Refresh();
        }

        #endregion

        #region Command Methods

        private void SelectShow(Show show)
        {
            var lastSeenSeasonNumber = 1;
            if (show.LastSeen != null)
            {
                lastSeenSeasonNumber = show.LastSeen.Season;
            }

            ServiceLocator.Current.GetInstance<ShowViewModel>().LoadShow(show.Id, lastSeenSeasonNumber);
            _hamburgerMenuService.NavigateTo(ViewConstants.Show);
        }

        #endregion

        #region Public Methods

        public void Refresh()
        {
            _allGroup.Shows.Clear();
            _lateGroup.Shows.Clear();
            _upToDateGroup.Shows.Clear();
            _continuingGroup.Shows.Clear();
            _endedGroup.Shows.Clear();
            _archivedGroup.Shows.Clear();

            _currentPage = 0;
            LastLoadingDate = DateTime.Now;

            LoadCollection();
        }

        #endregion

        #region Private Methods

        private void LoadCollection()
        {
            IsLoading = true;

            _tvshowtimeApiService.GetLibrary(_currentPage, _pageSize)
                .Subscribe(async (libraryResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        foreach (var show in libraryResponse.Shows)
                        {
                            // Do not add the same show twice
                            if (_allGroup.Shows.Any(s => s.Id == show.Id))
                                continue;

                            _allGroup.Shows.Add(show);

                            if (show.Archived.HasValue && show.Archived.Value)
                            {
                                _archivedGroup.Shows.Add(show);
                                continue;
                            }

                            if (show.Status == "Continuing")
                                _continuingGroup.Shows.Add(show);

                            if (show.Status == "Ended")
                                _endedGroup.Shows.Add(show);

                            if (show.LastAired != null)
                            {
                                if (show.LastSeen != null &&
                                    show.LastSeen.Season == show.LastAired.Season &&
                                    show.LastSeen.Number == show.LastAired.Number)
                                    _upToDateGroup.Shows.Add(show);
                                else
                                    _lateGroup.Shows.Add(show);
                            }
                        }

                        if (libraryResponse.Shows.Count >= _pageSize)
                        {
                            _currentPage++;
                            LoadCollection();
                        }
                        else
                        {
                            IsLoading = false;
                        }
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
