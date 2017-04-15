using System;
using System.Collections.ObjectModel;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class AgendaGroup
    {
        public DateTime Date { get; set; }
        public ObservableCollection<Episode> Episodes { get; } = new ObservableCollection<Episode>();
    }
}
