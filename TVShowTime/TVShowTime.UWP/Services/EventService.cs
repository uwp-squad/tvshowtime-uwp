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
        IObservable<Show> FollowShowEvent { get; }
        IObservable<Show> UnfollowShowEvent { get; }

        void WatchEpisode(Episode episode);
        void UnwatchEpisode(Episode episode);
        void FollowShow(Show show);
        void UnfollowShow(Show show);
    }

    public class EventService : IEventService
    {
        #region Fields

        private Subject<Episode> _watchEpisodeSubject = new Subject<Episode>();
        private Subject<Episode> _unwatchEpisodeSubject = new Subject<Episode>();
        private Subject<Show> _followShowSubject = new Subject<Show>();
        private Subject<Show> _unfollowShowSubject = new Subject<Show>();

        #endregion

        #region Properties

        public IObservable<Episode> WatchEpisodeEvent => _watchEpisodeSubject.AsObservable();
        public IObservable<Episode> UnwatchEpisodeEvent => _unwatchEpisodeSubject.AsObservable();
        public IObservable<Show> FollowShowEvent => _followShowSubject.AsObservable();
        public IObservable<Show> UnfollowShowEvent => _unfollowShowSubject.AsObservable();

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

        public void FollowShow(Show show)
        {
            _followShowSubject.OnNext(show);
        }

        public void UnfollowShow(Show show)
        {
            _unfollowShowSubject.OnNext(show);
        }

        #endregion
    }
}
