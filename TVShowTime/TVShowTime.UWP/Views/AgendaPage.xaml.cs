using System;
using System.Linq;
using System.Reactive.Linq;
using TVShowTime.UWP.Models;
using TVShowTime.UWP.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;

namespace TVShowTime.UWP.Views
{
    public sealed partial class AgendaPage : Page
    {
        #region Fields

        private AgendaViewModel _agendaViewModel;
        private double _scrollDelta = 100;

        #endregion

        #region Constructor

        public AgendaPage()
        {
            this.InitializeComponent();

            _agendaViewModel = (AgendaViewModel)DataContext;

            Loaded += OnLoaded;
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DetectScrollChange();
        }

        #endregion

        #region Methods

        private void DetectScrollChange()
        {
            var scrollViewer = EpisodesGridView.GetFirstDescendantOfType<ScrollViewer>();
            if (scrollViewer != null)
            {
                Observable.FromEventPattern<ScrollViewerViewChangedEventArgs>(x => scrollViewer.ViewChanged += x, x => scrollViewer.ViewChanged -= x)
                  .Select((eventPattern) =>
                  {
                      var sv = eventPattern.Sender as ScrollViewer;
                      return sv.VerticalOffset;
                  })
                  .Buffer(2, 1)
                  .Subscribe((offsets) =>
                  {
                      // Detect scroll direction
                      var offsetStart = offsets[0];
                      var offsetEnd = offsets[1];
                      var scrollDirection = (offsetEnd > offsetStart) ? ScrollDirection.TopToBottom : ScrollDirection.BottomToTop;

                      HandleScrollChange(scrollViewer, scrollDirection);
                  });
            }
        }

        private void HandleScrollChange(ScrollViewer scrollViewer, ScrollDirection scrollDirection)
        {
            double verticalOffset = scrollViewer.VerticalOffset;
            double maxVerticalOffset = scrollViewer.ScrollableHeight;

            if (maxVerticalOffset < 0)
            {
                // Nothing on the screen, load another page
                _agendaViewModel.LoadNextPage();
            }

            if (scrollDirection == ScrollDirection.TopToBottom && verticalOffset >= (maxVerticalOffset - _scrollDelta))
            {
                // Scrolled to bottom
                _agendaViewModel.LoadNextPage();
            }

            if (scrollDirection == ScrollDirection.BottomToTop && verticalOffset <= _scrollDelta)
            {
                // Scrolled to top
                _agendaViewModel.LoadPreviousPage();
            }
        }

        #endregion
    }
}
