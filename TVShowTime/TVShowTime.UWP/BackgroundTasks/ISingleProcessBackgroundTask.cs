using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace TVShowTime.UWP.BackgroundTasks
{
    public interface ISingleProcessBackgroundTask
    {
        Task RunAsync(IBackgroundTaskInstance taskInstance);
    }
}
