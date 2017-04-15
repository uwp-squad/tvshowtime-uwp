using System.Collections.ObjectModel;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class ShowCollectionGroup
    {
        public string Name { get; set; }
        public ObservableCollection<Show> Shows { get; } = new ObservableCollection<Show>();
    }
}
