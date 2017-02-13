using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Services
{
    public interface IEventService
    {
        IObservable<Episode> WatchEpisodeEvent { get; }
        IObservable<Episode> UnwatchEpisodeEvent { get; }

        void WatchEpisode(Episode episode);
        void UnwatchEpisode(Episode episode);
    }

    public class EventService : IEventService
    {
        #region Fields

        private Subject<Episode> _watchEpisodeSubject = new Subject<Episode>();
        private Subject<Episode> _unwatchEpisodeSubject = new Subject<Episode>();

        #endregion

        #region Properties

        public IObservable<Episode> WatchEpisodeEvent => _watchEpisodeSubject.AsObservable();
        public IObservable<Episode> UnwatchEpisodeEvent => _unwatchEpisodeSubject.AsObservable();

        #endregion

        #region Methods

        public void WatchEpisode(Episode episode)
        {
            _watchEpisodeSubject.OnNext(episode);
        }

        public void UnwatchEpisode(Episode episode)
        {
            _unwatchEpisodeSubject.OnNext(episode);
        }

        #endregion
    }
}
