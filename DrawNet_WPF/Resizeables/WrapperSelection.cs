using DrawNet_WPF.Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Xml.Linq;
using Vector = DrawNet_WPF.Converters.Vector;
using System.Diagnostics.Metrics;
using DrawNet_WPF.Converters;
using System.Linq;

namespace DrawNet_WPF.Resizeables
{
    /// <summary>
    /// Static class that regards all selection related functionalities.
    /// With this class, you are able to select multiple ResizeWrappers.
    /// </summary>
    public static class WrapperSelection
    {
        private static readonly List<ResizeWrapper> selected = new List<ResizeWrapper>();

        public static IReadOnlyList<ResizeWrapper> Selected => selected.AsReadOnly();
        private static readonly Dictionary<ResizeWrapper, DragInfo> dragInfoMap = new Dictionary<ResizeWrapper, DragInfo>();
        private static Window? window = null;

        public static void Select(ResizeWrapper item)
        {
            if (!selected.Contains(item))
            {
                if (window == null)
                {
                    window = WindowOperations.FindParentWindow(item);
                    window.MouseDown += WindowsMouseHandler;
                    window.KeyDown += WindowsKeyDown;
                }
                List<ResizeWrapper> prevList = new List<ResizeWrapper>(selected);
                item.AllowResize = true;
                selected.Add(item);
                List<ResizeWrapper> newList = new List<ResizeWrapper>(selected);
                item.TriggerSelect(prevList, newList);
                dragInfoMap[item] = new DragInfo { FixedPoint = null, FixedPointChange = 0, MovePoint = null, vectorFactor = null, RatioToPrimary = null };
            }                
        }

        public static void DeSelect(ResizeWrapper item)
        {
            List<ResizeWrapper> prevList = new List<ResizeWrapper>(selected);
            if (selected.Remove(item))
            {
                List<ResizeWrapper> newList = new List<ResizeWrapper>(selected);
                item.AllowResize = false;
                dragInfoMap.Remove(item);
                item.TriggerDeSelect(prevList, newList);
            }
        }

        public static void ClearSelection()
        {
            // Create a copy of the list to iterate over
            var itemsToRemove = selected.ToList();

            foreach (var item in itemsToRemove)
            {
                item.AllowResize = false;
                DeSelect(item); // This will modify the original 'selected' collection.
            }

            if (selected.Count != 0) { throw new Exception("Clear Selection Failed!"); }
            dragInfoMap.Clear();
        }


        public static bool isSelected(ResizeWrapper item)
        {
            return selected.Contains(item);
        }

        internal static void SyncBorder(ResizeWrapper master)
        {
            foreach(ResizeWrapper element in selected)
            {
                if (element == master) continue;
                DragInfo dragInfo = new DragInfo();
                dragInfo.vectorFactor = new Vector(1, 1);
                dragInfo.FixedPointChange = 1;
                dragInfo.FixedPoint = element.Position;
                dragInfo.MovePoint = element.Position + new Vector(element.Width, element.Height);
                dragInfo.RatioToPrimary = new Vector(element.Width / master.Width, element.Height / master.Height);
                dragInfoMap[element] = dragInfo;
                element.recursiveRelativeTransformSetter();
            }
        }
        internal static void SyncDiagonal(ResizeWrapper master, Vector direction)
        {
            foreach (ResizeWrapper element in selected)
            {
                if (element == master) continue;
                DragInfo dragInfo = new DragInfo();
                dragInfo.vectorFactor = new Vector(1, 1);
                dragInfo.FixedPointChange = 0;
                Vector MidPoint = element.Position + new Vector(element.Width / 2, element.Height / 2);
                dragInfo.MovePoint = MidPoint + (direction * new Vector(element.Width / 2, element.Height / 2));
                dragInfo.FixedPoint = MidPoint + (direction * new Vector(element.Width / 2, element.Height / 2)) * (-1);
                dragInfo.RatioToPrimary = new Vector(element.Width / master.Width, element.Height / master.Height);
                dragInfoMap[element] = dragInfo;
                element.recursiveRelativeTransformSetter();
            }
        }
        internal static void SyncLinear(ResizeWrapper master, Vector direction)
        {
            foreach (ResizeWrapper element in selected)
            {
                if (element == master) continue;
                DragInfo dragInfo = new DragInfo();
                dragInfo.vectorFactor = new Vector(direction.X == 0 ? 0 : 1, direction.Y == 0 ? 0 : 1);
                dragInfo.FixedPointChange = 0;
                Vector MidPoint = element.Position + new Vector(element.Width / 2, element.Height / 2);
                Vector RotateVector = new Vector(direction.X - direction.Y, direction.X + direction.Y); //rotates as if clockwise+1 rel to original handle
                dragInfo.MovePoint = MidPoint + (RotateVector * new Vector(element.Width / 2, element.Height / 2));
                dragInfo.FixedPoint = MidPoint + (RotateVector * new Vector(element.Width / 2, element.Height / 2)) * (-1);
                dragInfo.RatioToPrimary = new Vector(element.Width / master.Width, element.Height / master.Height);
                dragInfoMap[element] = dragInfo;
                element.recursiveRelativeTransformSetter();
            }
        }

        internal static void SyncTransform(ResizeWrapper master, Vector MouseMovement, Vector prevsize)
        {
            foreach(ResizeWrapper element in selected)
            {
                if (element == master) continue;
                else if (dragInfoMap.TryGetValue(element, out var dragInfo))
                {
                    if (dragInfo.FixedPoint != null && dragInfo.MovePoint != null && dragInfo.vectorFactor != null && dragInfo.RatioToPrimary != null)
                    {
                        Vector Movement = MouseMovement * dragInfo.RatioToPrimary;
                        Vector mp = Movement * dragInfo.vectorFactor + dragInfo.MovePoint;
                        Vector fp = Movement * dragInfo.FixedPointChange + dragInfo.FixedPoint;
                        Debug.WriteLine($"--forced to{element.Name}--\nmp: {mp.X}, {mp.Y} fp: {fp.X}, {fp.Y} mmove({MouseMovement.X},{MouseMovement.Y})");
                        Debug.Write($"Info: InfoMP({dragInfo.MovePoint.X},{dragInfo.MovePoint.Y}) InfoFP({dragInfo.FixedPoint.X},{dragInfo.FixedPoint.Y}) ");
                        Debug.WriteLine($"Movement({Movement.X},{Movement.Y}) vectorFactor({dragInfo.vectorFactor.X},{dragInfo.vectorFactor.Y}) fpchange({dragInfo.FixedPointChange})");
                        element.ResizeFromVectors(mp, fp);
                    }
                }
            }

        }

        internal static void SyncMouseUp(ResizeWrapper master)
        {
            foreach(ResizeWrapper element in selected)
            {
                if (element == master) continue;
                dragInfoMap[element] = new DragInfo { FixedPoint = null, FixedPointChange = 0, MovePoint = null, vectorFactor = null, RatioToPrimary = null };
            }
        }

        internal static bool WrapperMouseDown(ResizeWrapper sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (selected.Count == 0)
            {
                Select(sender);
                return false;
            }
            else
            {
                if (selected.Contains(sender))
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {//click already selected element, with ctrl
                        DeSelect(sender); return true;
                    }
                    else //click already selected element, with no ctrl
                    {
                        return false; 
                    }
                }
                else
                {
                    if(Keyboard.IsKeyDown(Key.LeftCtrl))
                    {//click unselected element, with ctrl
                        if (selected[0].outerPanel ==sender.outerPanel) Select(sender); 
                        return false;
                    }
                    else//click unselected element, with no ctrl
                    {
                        ClearSelection(); Select(sender); return true;
                    }
                }
            }
        }

        internal static void WindowsMouseHandler(object sender, MouseEventArgs e)
        {
            ClearSelection();
        }

        internal static void WindowsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClearSelection();
            }
        }

        internal struct DragInfo
        {
            #region DragHandle Control Variables
            internal Vector? FixedPoint;
            internal Vector? MovePoint;
            internal Vector? vectorFactor;
            internal Vector? RatioToPrimary;
            internal int FixedPointChange;
            #endregion
        }
    }
}
