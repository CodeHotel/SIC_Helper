using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawNet_WPF.Converters
{
    public class DimensionChangedEventArgs : EventArgs
    {
        public Vector OldSize { get; }
        public Vector NewSize { get; }
        public Vector OldPosition { get; }
        public Vector NewPosition { get; }

        public DimensionChangedEventArgs(Vector oldSize, Vector newSize, Vector oldPosition, Vector newPosition)
        {
            OldSize = oldSize;
            NewSize = newSize;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }
}
