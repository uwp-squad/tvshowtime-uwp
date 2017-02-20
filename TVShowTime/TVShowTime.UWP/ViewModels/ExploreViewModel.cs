using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private int _currentPage = 0;
        private bool _isLoadingShows = false;

        #endregion

        #region Properties

        public ObservableCollection<ExploreShowViewModel> Shows { get; } = new ObservableCollection<ExploreShowViewModel>();

        #endregion

        #region Commands

        public ICommand SelectionChangedCommand { get; }
        public ICommand ToggleFollowShowCommand { get; }

        #endregion

        #region Constructor

        public ExploreViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;

            SelectionChangedCommand = new RelayCommand<int>(SelectionChanged);
            ToggleFollowShowCommand = new RelayCommand<ExploreShowViewModel>(ToggleFollowShow);

            LoadTrendingShows(_currentPage);
        }

        #endregion

        #region Command Methods

        private void SelectionChanged(int currentIndex)
        {
            if (!_isLoadingShows && (Shows.Count - 3) <= currentIndex)
            {
                _currentPage++;
                LoadTrendingShows(_currentPage);
            }
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

        #endregion

        #region Private Methods

        private void LoadTrendingShows(int page)
        {
            _isLoadingShows = true;

            _tvshowtimeApiService.GetTrendingShows(page)
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

                        _isLoadingShows = false;
                    });
                },
                (error) =>
                {
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
