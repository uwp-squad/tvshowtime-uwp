using System;

namespace TVShowTime.UWP.Infrastructure
{
    public interface ILoadable
    {
        bool IsLoading { get; }
        DateTime LastLoadingDate { get; }
    }
}
