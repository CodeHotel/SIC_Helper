using DrawNet_WPF.Resizeables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawNet_WPF.Converters
{
    public class SelectionEventArgs : EventArgs
    {
        public SelectionEventArgs(List<ResizeWrapper> PreviousSelection, List<ResizeWrapper> NewSelection)
        {
            this.PreviousSelection = PreviousSelection;
            this.NewSelection = NewSelection;
        }

        public readonly List<ResizeWrapper> PreviousSelection;
        public readonly List<ResizeWrapper> NewSelection;
    }
}
