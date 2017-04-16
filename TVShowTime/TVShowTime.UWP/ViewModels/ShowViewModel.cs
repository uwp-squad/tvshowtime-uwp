using GalaSoft.MvvmLight;
using Microsoft.Toolkit.Uwp;
using System;
using TVShowTime.UWP.Services;
using TVShowTimeApi.Model;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.ViewModels
{
    public class ShowViewModel : ViewModelBase
    {
        #region Fields

        private IReactiveTVShowTimeApiService _tvshowtimeApiService;
        private IEventService _eventService;

        #endregion

        #region Properties

        public Show Show { get; set; }

        #endregion

        #region Constructor

        public ShowViewModel(
            IReactiveTVShowTimeApiService tvshowtimeApiService,
            IEventService eventService)
        {
            _tvshowtimeApiService = tvshowtimeApiService;
            _eventService = eventService;
        }

        #endregion

        #region Public Methods

        public void LoadShow(long showId)
        {
            RefreshByShowId(showId);
        }

        #endregion

        #region Private Methods

        private void RefreshByShowId(long showId)
        {
            _tvshowtimeApiService.GetShow(showId, string.Empty, false)
                .Subscribe(async (showResponse) =>
                {
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        Show = showResponse.Show;
                        RaisePropertyChanged(nameof(Show));
                    });
                },
                (error) =>
                {
                    throw new Exception();
                });
        }

        #endregion
    }
}
