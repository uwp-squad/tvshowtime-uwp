using System.Collections.ObjectModel;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class ShowSeasonGroup
    {
        public int SeasonNumber { get; set; }
        public ObservableCollection<Episode> Episodes { get; } = new ObservableCollection<Episode>();
    }
}
