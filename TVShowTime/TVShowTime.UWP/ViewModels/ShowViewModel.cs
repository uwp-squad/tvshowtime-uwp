using GalaSoft.MvvmLight;
using Microsoft.Toolkit.Uwp;
using System;
using TVShowTime.UWP.Infrastructure;
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

        #endregion

        #region Properties

        public Show Show { get; set; }

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

        #region Constructor

        public ShowViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService,
            IToastNotificationService toastNotificationService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;
            _toastNotificationService = toastNotificationService;
        }

        #endregion

        #region Public Methods

        public void LoadShow(long showId)
        {
            RefreshByShowId(showId);
        }

        public void Refresh()
        {
            RefreshByShowId(Show.Id);
        }

        #endregion

        #region Private Methods

        private void RefreshByShowId(long showId)
        {
            IsLoading = true;

            _tvshowtimeApiService.GetShow(showId, string.Empty, false)
                .Subscribe(async (showResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        LastLoadingDate = DateTime.Now;

                        Show = showResponse.Show;
                        RaisePropertyChanged(nameof(Show));

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
