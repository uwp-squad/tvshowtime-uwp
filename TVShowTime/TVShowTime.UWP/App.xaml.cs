﻿using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TVShowTime.UWP.Views;
using Microsoft.QueryStringDotNET;
using TVShowTime.UWP.Constants;
using Microsoft.Practices.ServiceLocation;
using TVShowTime.UWP.ViewModels;
using Microsoft.Toolkit.Uwp;
using System.Reflection;
using TVShowTime.UWP.BackgroundTasks;
using System.Threading.Tasks;
using TVShowTime.UWP.Infrastructure;
using TVShowTime.UWP.Services;

namespace TVShowTime.UWP
{
    /// <summary>
    /// Fournit un comportement spécifique à l'application afin de compléter la classe Application par défaut.
    /// </summary>
    sealed partial class App : Application
    {
        #region Constructor

        /// <summary>
        /// Initialise l'objet d'application de singleton.  Il s'agit de la première ligne du code créé
        /// à être exécutée. Elle correspond donc à l'équivalent logique de main() ou WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoqué lorsque l'application est lancée normalement par l'utilisateur final.  D'autres points d'entrée
        /// seront utilisés par exemple au moment du lancement de l'application pour l'ouverture d'un fichier spécifique.
        /// </summary>
        /// <param name="e">Détails concernant la requête et le processus de lancement.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            bool resuming = true;
            var rootFrame = Window.Current.Content as Frame;

            // Ne répétez pas l'initialisation de l'application lorsque la fenêtre comporte déjà du contenu, assurez-vous juste que la fenêtre est active
            if (rootFrame == null)
            {
                // Créez un Frame utilisable comme contexte de navigation et naviguez jusqu'à la première page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: chargez l'état de l'application précédemment suspendue
                }

                // Placez le frame dans la fenêtre active
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Quand la pile de navigation n'est pas restaurée, accédez à la première page,
                    // puis configurez la nouvelle page en transmettant les informations requises en tant que paramètre
                    rootFrame.Navigate(typeof(LoginPage), e.Arguments);

                    resuming = false;
                }

                // Vérifiez que la fenêtre actuelle est active
                Window.Current.Activate();

                if (resuming)
                {
                    // Handle refresh of current page
                    HandleRefresh();
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            // Get the root frame
            Frame rootFrame = Window.Current.Content as Frame;

            // Handle toast activation
            if (args is ToastNotificationActivatedEventArgs toastNotificationActivatedEventArgs)
            {
                HandleToastActivation(toastNotificationActivatedEventArgs);
            }

            // Ne répétez pas l'initialisation de l'application lorsque la fenêtre comporte déjà du contenu, assurez-vous juste que la fenêtre est active
            if (rootFrame == null)
            {
                // Créez un Frame utilisable comme contexte de navigation et naviguez jusqu'à la première page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: chargez l'état de l'application précédemment suspendue
                }

                // Placez le frame dans la fenêtre active
                Window.Current.Content = rootFrame;

            }

            if (rootFrame.Content == null)
            {
                // Quand la pile de navigation n'est pas restaurée, accédez à la première page,
                // puis configurez la nouvelle page en transmettant les informations requises en tant que paramètre
                rootFrame.Navigate(typeof(LoginPage));
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Appelé lorsque la navigation vers une page donnée échoue
        /// </summary>
        /// <param name="sender">Frame à l'origine de l'échec de navigation.</param>
        /// <param name="e">Détails relatifs à l'échec de navigation</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Appelé lorsque l'exécution de l'application est suspendue.  L'état de l'application est enregistré
        /// sans savoir si l'application pourra se fermer ou reprendre sans endommager
        /// le contenu de la mémoire.
        /// </summary>
        /// <param name="sender">Source de la requête de suspension.</param>
        /// <param name="e">Détails de la requête de suspension.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: enregistrez l'état de l'application et arrêtez toute activité en arrière-plan
            deferral.Complete();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            await HandleBackgroundTaskActivationAsync(args, typeof(App));
        }

        #endregion

        #region Private methods

        private void HandleToastActivation(ToastNotificationActivatedEventArgs args)
        {
            // Parse the query string
            var query = QueryString.Parse(args.Argument);

            query.TryGetValue("action", out string action);

            // See what action is being requested 
            switch (action)
            {
                case NotificationConstants.AlertNewEpisode:
                    if (long.TryParse(query["episodeId"], out long episodeId))
                    {
                        new LocalObjectStorageHelper().Save(LocalStorageConstants.NavigateToEpisode, episodeId);

                        if (args.PreviousExecutionState == ApplicationExecutionState.Running ||
                            args.PreviousExecutionState == ApplicationExecutionState.Suspended)
                        {
                            ServiceLocator.Current.GetInstance<MainViewModel>().HandleActivation();
                        }
                    }
                    break;
            }
        }

        private async Task HandleBackgroundTaskActivationAsync(BackgroundActivatedEventArgs args, Type appType)
        {
            var deferral = args.TaskInstance.GetDeferral();

            var assembly = appType.GetTypeInfo().Assembly;
            var type = assembly.GetTypes()
                .Where(t => t.GetTypeInfo().IsClass && t.Name == args.TaskInstance.Task.Name)
                .FirstOrDefault();

            if (type != null)
            {
                var task = Activator.CreateInstance(type) as ISingleProcessBackgroundTask;
                await task.RunAsync(args.TaskInstance);
            }

            deferral.Complete();
        }

        private void HandleRefresh()
        {
            var hamburgerMenuService = ServiceLocator.Current.GetInstance<IHamburgerMenuService>();
            if (hamburgerMenuService != null)
            {
                var internalFrame = hamburgerMenuService.GetFrameElement();
                if (internalFrame.Content is Page internalPage)
                {
                    if (internalPage.DataContext is IRefreshable refreshableViewModel)
                    {
                        if (refreshableViewModel.CanRefresh && refreshableViewModel.ShouldRefresh)
                        {
                            refreshableViewModel.Refresh();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
