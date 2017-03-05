using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class ShowCollectionGroup
    {
        public string Name { get; set; }
        public ObservableCollection<Show> Shows { get; } = new ObservableCollection<Show>();
    }
}
