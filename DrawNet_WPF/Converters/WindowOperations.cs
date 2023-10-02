using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace DrawNet_WPF.Converters
{
    public static class WindowOperations
    {
        public static Window? FindParentWindow(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) // You can reach null if the object has no parent, or if the object is a top-level element, we can try logical tree then.
                return null;

            if (parentObject is Window parent)
                return parent;
            else
                return FindParentWindow(parentObject);
        }

    }
}
