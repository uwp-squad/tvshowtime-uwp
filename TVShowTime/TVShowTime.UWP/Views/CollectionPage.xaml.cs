﻿using TVShowTime.UWP.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TVShowTime.UWP.Views
{
    public sealed partial class CollectionPage : Page
    {
        #region Properties

        public CollectionViewModel ViewModel { get; }

        #endregion

        #region Constructor

        public CollectionPage()
        {
            InitializeComponent();

            ViewModel = (CollectionViewModel)DataContext;
        }

        #endregion

        #region Events

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Each time a navigation event occurs, update the Back button's visibility
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        #endregion
    }
}
