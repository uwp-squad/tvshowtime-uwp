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
    public class ExploreViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IEventService _eventService;
        private IHamburgerMenuService _hamburgerMenuService;

        private const int _pageSize = 5;
        private int _currentPage = 0;
        private int _currentIndex = 0;

        #endregion

        #region Properties

        public ObservableCollection<ExploreShowViewModel> Shows { get; } = new ObservableCollection<ExploreShowViewModel>();

        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ShowLoading));
            }
        }

        public bool ShowLoading { get { return IsLoading && (Shows.Count - 1) == _currentIndex; } }

        #endregion

        #region Commands

        public ICommand SelectionChangedCommand { get; }
        public ICommand ToggleFollowShowCommand { get; }
        public ICommand SelectShowCommand { get; }

        #endregion

        #region Constructor

        public ExploreViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService,
            IHamburgerMenuService hamburgerMenuService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;
            _hamburgerMenuService = hamburgerMenuService;

            SelectionChangedCommand = new RelayCommand<int>(SelectionChanged);
            ToggleFollowShowCommand = new RelayCommand<ExploreShowViewModel>(ToggleFollowShow);
            SelectShowCommand = new RelayCommand<long>(SelectShow);

            LoadTrendingShows(_currentPage);
        }

        #endregion

        #region Command Methods

        private void SelectionChanged(int currentIndex)
        {
            _currentIndex = currentIndex;
            if (!IsLoading && (Shows.Count - _pageSize) <= _currentIndex)
            {
                _currentPage++;
                LoadTrendingShows(_currentPage);
            }

            RaisePropertyChanged(nameof(ShowLoading));
        }

        private void ToggleFollowShow(ExploreShowViewModel show)
        {
            if (show.Followed.HasValue && show.Followed.Value)
            {
                // Unfollow
                _tvshowtimeApiService.UnfollowShow(show.Id)
                    .Subscribe(async (response) =>
                    {
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            show.Followed = false;
                            show.Original.Followed = false;
                            _eventService.UnfollowShow(show.Original);
                        });
                    },
                    (error) =>
                    {
                        throw new Exception();
                    });
            }
            else
            {
                // Follow
                _tvshowtimeApiService.FollowShow(show.Id)
                    .Subscribe(async (response) =>
                    {
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            show.Followed = true;
                            show.Original.Followed = true;
                            _eventService.FollowShow(show.Original);
                        });
                    },
                    (error) =>
                    {
                        throw new Exception();
                    });
            }
        }

        private void SelectShow(long showId)
        {
            ServiceLocator.Current.GetInstance<ShowViewModel>().LoadShow(showId);
            _hamburgerMenuService.NavigateTo(ViewConstants.Show);
        }

        #endregion

        #region Private Methods

        private void LoadTrendingShows(int page)
        {
            IsLoading = true;

            _tvshowtimeApiService.GetTrendingShows(page, _pageSize)
                .Subscribe(async (exploreResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        foreach (var show in exploreResponse.Shows)
                        {
                            // Do not add the same show twice
                            if (Shows.Any(s => s.Id == show.Id))
                                continue;

                            var exploreShowViewModel = new ExploreShowViewModel
                            {
                                Id = show.Id,
                                Name = show.Name,
                                Overview = show.Overview,
                                AllImages = show.AllImages,
                                Followed = show.Followed,
                                Original = show
                            };
                            Shows.Add(exploreShowViewModel);
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

        #region Private ViewModels

        public class ExploreShowViewModel : ViewModelBase
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Overview { get; set; }
            public ShowImages AllImages { get; set; }

            private bool? _followed;
            public bool? Followed
            {
                get { return _followed; }
                set { _followed = value; RaisePropertyChanged(); }
            }

            public Show Original { get; set; }
        }

        #endregion
    }
}
