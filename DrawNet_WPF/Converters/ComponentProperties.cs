using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawNet_WPF.Converters
{
    public static class ComponentProperties
    {
        public static readonly DependencyProperty ComponentTypeProperty = DependencyProperty.RegisterAttached(
            "ComponentType",
            typeof(ComponentType),
            typeof(ComponentProperties),
            new PropertyMetadata(ComponentType.None)); // Default value set to None

        public static void SetType(UIElement element, ComponentType value)
        {
            element.SetValue(ComponentTypeProperty, value);
        }

        public static ComponentType GetType(UIElement element)
        {
            return (ComponentType)element.GetValue(ComponentTypeProperty);
        }
    }
    public enum ComponentType
    {
        None,
        ResizeHandle,
        RotateHandle,
        ResizeWrapper,
        ResizeHandleEllipse,
        ResizeHandleRectangle,
        RotateHandleEllipse,
        RotateHandleRectangle,
        ResizeWrapperBorder
    }

}
