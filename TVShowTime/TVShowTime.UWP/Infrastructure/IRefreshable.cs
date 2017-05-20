namespace TVShowTime.UWP.Infrastructure
{
    public interface IRefreshable
    {
        bool CanRefresh { get; }
        bool ShouldRefresh { get; }

        void Refresh();
    }
}
