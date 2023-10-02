using System;
using System.Configuration;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DrawNet_WPF.Converters;
using DrawNet_WPF.Resizeables;
using Vector = DrawNet_WPF.Converters.Vector;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace DrawNet_WPF.Handles
{
    internal class ResizeHandle : Control
    {

        internal Rectangle _rectangle;
        internal Ellipse _ellipse;
        public event MouseButtonEventHandler HandleMouseDown;

        public ResizeHandle()
        {
            ComponentProperties.SetType(this, ComponentType.ResizeHandle);
            _rectangle = new Rectangle();
            _ellipse = new Ellipse();
            ComponentProperties.SetType(_rectangle, ComponentType.ResizeHandleRectangle);
            ComponentProperties.SetType(_ellipse, ComponentType.ResizeHandleEllipse);
            _ellipse.MouseDown += OnShapeMouseDown;
            _rectangle.MouseDown += OnShapeMouseDown;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateShape();



        }
        private void UpdateShape()
        {
            if (Shape == ShapeType.Square)
            {
                _ellipse.Visibility = Visibility.Collapsed;
                _rectangle.Visibility = Visibility.Visible;
            }
            else
            {
                _rectangle.Visibility = Visibility.Collapsed;
                _ellipse.Visibility = Visibility.Visible;
            }
        }

        // Trigger UpdateShape when the DependencyProperty changes.
        private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ResizeHandle handle)
            {
                handle.UpdateShape();
            }
        }

        internal void InitHandle(Panel panel, ResizeWrapper parent, ShapeType HandleShape, double HandleSize, Brush HandleStroke, double HandleStrokeThickness, Brush HandleFill)
        {
            if (setHandleProperty(parent, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill))
            {
                panel.Children.Add(_ellipse);
                panel.Children.Add(_rectangle);
                UpdateShape();
                //add mousedown handlers to 
            }
        }

        internal bool setHandleProperty(ResizeWrapper parent, ShapeType HandleShape, double HandleSize, Brush HandleStroke, double HandleStrokeThickness, Brush HandleFill)
        {
            double parentThickness = parent.StrokeThickness;
            double parentWidth = parent.Width;
            double parentHeight = parent.Height;
            double parentX = parent.Position.X;
            double parentY = parent.Position.Y;

            if (double.IsNaN(parentWidth) || double.IsNaN(parentHeight) || double.IsNaN(parentX) || double.IsNaN(parentY)) 
                return false;


            Vector newPos = new Vector(
                MovType.X * (parentWidth / 2 + parentThickness / 2) + parentX + parentWidth / 2 - HandleSize / 2,
                MovType.Y * (parentHeight / 2 + parentThickness / 2) + parentY + parentHeight / 2 - HandleSize / 2);
            SetPos(newPos, this); SetPos(newPos, _ellipse); SetPos(newPos, _rectangle);
            _ellipse.Width = _ellipse.Height = _rectangle.Width = _rectangle.Height = HandleSize;
            _ellipse.Stroke = _rectangle.Stroke = HandleStroke;
            _ellipse.StrokeThickness = _rectangle.StrokeThickness = HandleStrokeThickness;
            _ellipse.Fill = _rectangle.Fill = HandleFill;
            Shape = HandleShape;
            return true;
        }

        static ResizeHandle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeHandle), new FrameworkPropertyMetadata(typeof(ResizeHandle)));
        }

        public ResizeWrapper ParentControl { get; set; }
        internal Vector MovType { get; set; }
        internal Panel outerPanel { get; set; }

        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register(nameof(Shape), typeof(ShapeType), typeof(ResizeHandle), new PropertyMetadata(ShapeType.Square));
        public ShapeType Shape
        {
            get => (ShapeType)GetValue(ShapeProperty);
            set {
                SetValue(ShapeProperty, value);
                UpdateShape();
            }
        }

        private Vector _position;
        public Vector Position
        {
            get => _position ?? new Vector(0, 0);
            set
            {
                _position = value ?? throw new ArgumentNullException(nameof(value));
                SetPos(_position, this);
            }
        }
        private void SetPos(Vector v, UIElement target)
        {
            Canvas.SetLeft(target, v.X);
            Canvas.SetTop(target, v.Y);
        }
        private void OnShapeMouseDown(object sender, MouseButtonEventArgs e)
        {
            HandleMouseDown?.Invoke(this, e);
        }
    }

    public enum ShapeType
    {
        Square,
        Circle
    }
}
