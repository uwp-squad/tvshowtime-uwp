using GalaSoft.MvvmLight;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTime.UWP.BackgroundTasks;
using Windows.ApplicationModel.Background;

namespace TVShowTime.UWP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Constructor

        public MainViewModel()
        {
            RegisterBackgroundTasks();
        }

        #endregion

        #region Private methods

        private void RegisterBackgroundTasks()
        {
            // Check if Background Task is already registered
            if (!BackgroundTaskHelper.IsBackgroundTaskRegistered(typeof(NewEpisodesBackgroundTask)))
            {
                BackgroundTaskHelper.Register(
                    typeof(NewEpisodesBackgroundTask).Name,
                    new TimeTrigger(120, false),
                    conditions: new SystemCondition(SystemConditionType.InternetAvailable)
                );
            }
        }

        #endregion
    }
}
