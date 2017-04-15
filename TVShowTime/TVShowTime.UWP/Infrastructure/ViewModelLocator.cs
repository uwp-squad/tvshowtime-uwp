using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Toolkit.Uwp;
using TVShowTime.UWP.Constants;
using TVShowTime.UWP.Services;
using TVShowTime.UWP.ViewModels;
using TVShowTime.UWP.Views;
using TVShowTimeApi.Services;

namespace TVShowTime.UWP.Infrastructure
{
    public class ViewModelLocator
    {
        #region Constructor

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Register Services
            if (!SimpleIoc.Default.IsRegistered<INavigationService>())
            {
                var navigationService = CreateNavigationService();
                SimpleIoc.Default.Register(() => navigationService);
            }

            if (!SimpleIoc.Default.IsRegistered<IReactiveTVShowTimeApiService>())
            {
                var tvshowtimeApiService = new ReactiveTVShowTimeApiService();
                SimpleIoc.Default.Register<IReactiveTVShowTimeApiService>(() => tvshowtimeApiService);
            }

            if (!SimpleIoc.Default.IsRegistered<IHamburgerMenuService>())
            {
                var hamburgerMenuService = new HamburgerMenuService();
                SimpleIoc.Default.Register<IHamburgerMenuService>(() => hamburgerMenuService);
            }

            if (!SimpleIoc.Default.IsRegistered<IObjectStorageHelper>())
            {
                var localObjectStorageHelper = new LocalObjectStorageHelper();
                SimpleIoc.Default.Register<IObjectStorageHelper>(() => localObjectStorageHelper, ServiceLocatorConstants.LocalObjectStorageHelper);

                var roamingObjectStorageHelper = new RoamingObjectStorageHelper();
                SimpleIoc.Default.Register<IObjectStorageHelper>(() => roamingObjectStorageHelper, ServiceLocatorConstants.RoamingObjectStorageHelper);
            }

            if (!SimpleIoc.Default.IsRegistered<IEventService>())
            {
                var eventService = new EventService();
                SimpleIoc.Default.Register<IEventService>(() => eventService);
            }

            // Register ViewModels
            SimpleIoc.Default.Register<AgendaViewModel>();
            SimpleIoc.Default.Register<CollectionViewModel>();
            SimpleIoc.Default.Register<EpisodeViewModel>();
            SimpleIoc.Default.Register<ExploreViewModel>();
            SimpleIoc.Default.Register<FeedbackViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ShowViewModel>();
            SimpleIoc.Default.Register<ToWatchViewModel>();
            SimpleIoc.Default.Register<UpcomingViewModel>();
        }

        #endregion

        #region Methods

        private static INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();

            navigationService.Configure(ViewConstants.Login, typeof(LoginPage));
            navigationService.Configure(ViewConstants.Main, typeof(MainPage));

            return navigationService;
        }

        #endregion

        #region ViewModels

        public AgendaViewModel Agenda
        {
            get { return ServiceLocator.Current.GetInstance<AgendaViewModel>(); }
        }

        public CollectionViewModel Collection
        {
            get { return ServiceLocator.Current.GetInstance<CollectionViewModel>(); }
        }

        public EpisodeViewModel Episode
        {
            get { return ServiceLocator.Current.GetInstance<EpisodeViewModel>(); }
        }

        public ExploreViewModel Explore
        {
            get { return ServiceLocator.Current.GetInstance<ExploreViewModel>(); }
        }

        public FeedbackViewModel Feedback
        {
            get { return ServiceLocator.Current.GetInstance<FeedbackViewModel>(); }
        }

        public LoginViewModel Login
        {
            get { return ServiceLocator.Current.GetInstance<LoginViewModel>(); }
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public ShowViewModel Show
        {
            get { return ServiceLocator.Current.GetInstance<ShowViewModel>(); }
        }

        public ToWatchViewModel ToWatch
        {
            get { return ServiceLocator.Current.GetInstance<ToWatchViewModel>(); }
        }

        public UpcomingViewModel Upcoming
        {
            get { return ServiceLocator.Current.GetInstance<UpcomingViewModel>(); }
        }

        #endregion
    }
}
