using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TVShowTimeApi.Model;

namespace TVShowTime.UWP.Models
{
    public class AgendaGroup
    {
        public DateTime Date { get; set; }
        public ObservableCollection<Episode> Episodes { get; set; }
    }
}
