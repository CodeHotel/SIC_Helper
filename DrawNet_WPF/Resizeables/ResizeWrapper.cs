using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DrawNet_WPF.Converters;
using DrawNet_WPF.Handles;
using Vector = DrawNet_WPF.Converters.Vector;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace DrawNet_WPF.Resizeables
{
    public class ResizeWrapper : Canvas
    {
        static ResizeWrapper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeWrapper), new FrameworkPropertyMetadata(typeof(ResizeWrapper)));
        }

        public ResizeWrapper()
        {
            #region Throws Exception if Margin is not 0

            var dpd = DependencyPropertyDescriptor.FromProperty(ResizeWrapper.MarginProperty, typeof(ResizeWrapper));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, (s, e) =>
                {
                    if (Margin != new Thickness(0))
                        throw new InvalidOperationException("Margin must be zero!");
                });
            }
            #endregion

            ComponentProperties.SetType(this, ComponentType.ResizeWrapper);
            ClipToBounds = false;
            Loaded += ResizeWrapper_Loaded;
        }

        private void ResizeWrapper_Loaded(object sender, RoutedEventArgs e)
        {
            #region Gets closest ancestor that is Panel. Throws exception if not found
            var parent = VisualTreeHelper.GetParent(this);

            Canvas? foundPanel = null;

            while (parent != null)
            {
                if (parent is Canvas panel)
                {
                    foundPanel = panel;
                    break;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }
            if (foundPanel == null) throw new Exception("ResizeWrapper must be inside a Canvas(ResizeWrapper, ResizeCanvas) to function correctly");
            else outerPanel = foundPanel;
            #endregion

            InitializeOuterBorder();

            outerPanel.Children.Remove(this);
            outerPanel.Children.Add(_outerBorder);
            _outerBorder.Child = this;
            if (AllowDrag) SubscribeBorder();
            else UnsubscribeBorder();
            InitializeHandles(HandleShape);

            Window? window = Converters.WindowOperations.FindParentWindow(this);
            if (window != null)
            {
                window.MouseMove += WindowsMouseMove;
                window.MouseUp += WindowsMouseUp;
            }

            if (UseSelection)
            {
                AllowResize = false;
                AllowDrag = true;
            }
            else
            {
                AllowResize = true;
                AllowDrag = true;
            }

            this.Loaded -= ResizeWrapper_Loaded;
        }

        private void InitializeHandles(ShapeType HandleShape)
        {
            Handles = new ResizeHandle[3, 3];
            Handles[0, 0] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(-1, -1) };
            Handles[1, 0] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(0, -1) };
            Handles[2, 0] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(1, -1) };
            Handles[0, 1] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(-1, 0) };
            Handles[1, 1] = null;
            Handles[2, 1] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(1, 0) };
            Handles[0, 2] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(-1, 1) };
            Handles[1, 2] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(0, 1) };
            Handles[2, 2] = new ResizeHandle { ParentControl = this, outerPanel = outerPanel, MovType = new Vector(1, 1) };
            RotationHandle = new RotationHandle { ParentControl = this, outerPanel = outerPanel, Shape = HandleShape };

            outerPanel.Children.Add(Handles[0, 0]);
            outerPanel.Children.Add(Handles[1, 0]);
            outerPanel.Children.Add(Handles[2, 0]);
            outerPanel.Children.Add(Handles[0, 1]);
            outerPanel.Children.Add(Handles[2, 1]);
            outerPanel.Children.Add(Handles[0, 2]);
            outerPanel.Children.Add(Handles[1, 2]);
            outerPanel.Children.Add(Handles[2, 2]);
            outerPanel.Children.Add(RotationHandle);
            if(AllowResize) EnableHandle();
            else DisableHandle();
            InitHandles();
        }

        private void EnableHandle()
        {
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (Handles == null) return;
            Handles[0, 0].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[1, 0].HandleMouseDown -= LinearMouseDownHandler;
            Handles[2, 0].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[0, 1].HandleMouseDown -= LinearMouseDownHandler;
            Handles[2, 1].HandleMouseDown -= LinearMouseDownHandler;
            Handles[0, 2].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[1, 2].HandleMouseDown -= LinearMouseDownHandler;
            Handles[2, 2].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[0, 0].HandleMouseDown += DiagonalMouseDownHandler;
            Handles[1, 0].HandleMouseDown += LinearMouseDownHandler;
            Handles[2, 0].HandleMouseDown += DiagonalMouseDownHandler;
            Handles[0, 1].HandleMouseDown += LinearMouseDownHandler;
            Handles[2, 1].HandleMouseDown += LinearMouseDownHandler;
            Handles[0, 2].HandleMouseDown += DiagonalMouseDownHandler;
            Handles[1, 2].HandleMouseDown += LinearMouseDownHandler;
            Handles[2, 2].HandleMouseDown += DiagonalMouseDownHandler;

            Handles[0, 0]._ellipse.Visibility = Visibility.Visible;
            Handles[0, 0]._rectangle.Visibility = Visibility.Visible;
            Handles[1, 0]._ellipse.Visibility = Visibility.Visible;
            Handles[1, 0]._rectangle.Visibility = Visibility.Visible;
            Handles[2, 0]._ellipse.Visibility = Visibility.Visible;
            Handles[2, 0]._rectangle.Visibility = Visibility.Visible;
            Handles[0, 1]._ellipse.Visibility = Visibility.Visible;
            Handles[0, 1]._rectangle.Visibility = Visibility.Visible;
            Handles[2, 1]._ellipse.Visibility = Visibility.Visible;
            Handles[2, 1]._rectangle.Visibility = Visibility.Visible;
            Handles[0, 2]._ellipse.Visibility = Visibility.Visible;
            Handles[0, 2]._rectangle.Visibility = Visibility.Visible;
            Handles[1, 2]._ellipse.Visibility = Visibility.Visible;
            Handles[1, 2]._rectangle.Visibility = Visibility.Visible;
            Handles[2, 2]._ellipse.Visibility = Visibility.Visible;
            Handles[2, 2]._rectangle.Visibility = Visibility.Visible;

            Handles[0, 0]._ellipse.Cursor = Cursors.SizeNWSE;
            Handles[1, 0]._ellipse.Cursor = Cursors.SizeNS;
            Handles[2, 0]._ellipse.Cursor = Cursors.SizeNESW;
            Handles[0, 1]._ellipse.Cursor = Cursors.SizeWE;
            Handles[2, 1]._ellipse.Cursor = Cursors.SizeWE;
            Handles[0, 2]._ellipse.Cursor = Cursors.SizeNESW;
            Handles[1, 2]._ellipse.Cursor = Cursors.SizeNS;
            Handles[2, 2]._ellipse.Cursor = Cursors.SizeNWSE;

            Handles[0, 0]._rectangle.Cursor = Cursors.SizeNWSE;
            Handles[1, 0]._rectangle.Cursor = Cursors.SizeNS;
            Handles[2, 0]._rectangle.Cursor = Cursors.SizeNESW;
            Handles[0, 1]._rectangle.Cursor = Cursors.SizeWE;
            Handles[2, 1]._rectangle.Cursor = Cursors.SizeWE;
            Handles[0, 2]._rectangle.Cursor = Cursors.SizeNESW;
            Handles[1, 2]._rectangle.Cursor = Cursors.SizeNS;
            Handles[2, 2]._rectangle.Cursor = Cursors.SizeNWSE;
            UpdateHandles();
            #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private void DisableHandle()
        {
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (Handles == null) return;
            Handles[0, 0].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[1, 0].HandleMouseDown -= LinearMouseDownHandler;
            Handles[2, 0].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[0, 1].HandleMouseDown -= LinearMouseDownHandler;
            Handles[2, 1].HandleMouseDown -= LinearMouseDownHandler;
            Handles[0, 2].HandleMouseDown -= DiagonalMouseDownHandler;
            Handles[1, 2].HandleMouseDown -= LinearMouseDownHandler;
            Handles[2, 2].HandleMouseDown -= DiagonalMouseDownHandler;

            Handles[0, 0]._ellipse.Visibility = Visibility.Collapsed;
            Handles[0, 0]._rectangle.Visibility = Visibility.Collapsed;
            Handles[1, 0]._ellipse.Visibility = Visibility.Collapsed;
            Handles[1, 0]._rectangle.Visibility = Visibility.Collapsed;
            Handles[2, 0]._ellipse.Visibility = Visibility.Collapsed;
            Handles[2, 0]._rectangle.Visibility = Visibility.Collapsed;
            Handles[0, 1]._ellipse.Visibility = Visibility.Collapsed;
            Handles[0, 1]._rectangle.Visibility = Visibility.Collapsed;
            Handles[2, 1]._ellipse.Visibility = Visibility.Collapsed;
            Handles[2, 1]._rectangle.Visibility = Visibility.Collapsed;
            Handles[0, 2]._ellipse.Visibility = Visibility.Collapsed;
            Handles[0, 2]._rectangle.Visibility = Visibility.Collapsed;
            Handles[1, 2]._ellipse.Visibility = Visibility.Collapsed;
            Handles[1, 2]._rectangle.Visibility = Visibility.Collapsed;
            Handles[2, 2]._ellipse.Visibility = Visibility.Collapsed;
            Handles[2, 2]._rectangle.Visibility = Visibility.Collapsed;

            Handles[0, 0]._ellipse.Cursor = null;
            Handles[1, 0]._ellipse.Cursor = null;
            Handles[2, 0]._ellipse.Cursor = null;
            Handles[0, 1]._ellipse.Cursor = null;
            Handles[2, 1]._ellipse.Cursor = null;
            Handles[0, 2]._ellipse.Cursor = null;
            Handles[1, 2]._ellipse.Cursor = null;
            Handles[2, 2]._ellipse.Cursor = null;

            Handles[0, 0]._rectangle.Cursor = null;
            Handles[1, 0]._rectangle.Cursor = null;
            Handles[2, 0]._rectangle.Cursor = null;
            Handles[0, 1]._rectangle.Cursor = null;
            Handles[2, 1]._rectangle.Cursor = null;
            Handles[0, 2]._rectangle.Cursor = null;
            Handles[1, 2]._rectangle.Cursor = null;
            Handles[2, 2]._rectangle.Cursor = null;
            #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private void SubscribeBorder()
        {
            if (_outerBorder == null) return;
            _outerBorder.MouseDown -= BorderMouseDownHandler;
            _outerBorder.MouseDown += BorderMouseDownHandler;
            _outerBorder.Cursor = Cursors.SizeAll;
        }

        private void UnsubscribeBorder()
        {
            if (_outerBorder == null) return;
            _outerBorder.MouseDown -= BorderMouseDownHandler;
            _outerBorder.Cursor = null;
        }

        private void InitHandles()
        {
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            Handles[0, 0].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[1, 0].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[2, 0].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[0, 1].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[2, 1].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[0, 2].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[1, 2].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[2, 2].InitHandle(outerPanel, this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private void UpdateHandles()
        {
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            Handles[0, 0].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[1, 0].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[2, 0].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[0, 1].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[2, 1].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[0, 2].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[1, 2].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            Handles[2, 2].setHandleProperty(this, HandleShape, HandleSize, HandleStroke, HandleStrokeThickness, HandleFill);
            #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        private void setRotationProperty(bool RotationEnabled, double RotationOffset)
        {
            if (RotationEnabled) RotationHandle.Visibility = Visibility.Visible;
            else RotationHandle.Visibility = Visibility.Collapsed;
            //TODO: declare the line that connects rotational handle and top middle handle at the bottom of class. add logic to manipulate it.
        }

        private void InitializeOuterBorder()
        {
            if (_outerBorder == null)
            {
                _outerBorder = new Border();
                ComponentProperties.SetType(_outerBorder, ComponentType.ResizeWrapperBorder);
            }

            // Set the border to surround the control
            _outerBorder.BorderBrush = Stroke;
            _outerBorder.BorderThickness = new Thickness(StrokeThickness);
            _outerBorder.Width = Width + 2 * StrokeThickness;
            _outerBorder.Height = Height + 2 * StrokeThickness;
            Position = Position;
        }

        #region Properties
        //The position of the Wrapper, relative to closest parent that is a Panel
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(Vector), typeof(ResizeWrapper), new PropertyMetadata(new Vector(50, 50)));
        /// <summary>
        /// The position("x,y") of the Wrapper, relative to closest parent that is a Panel
        /// </summary>
        public Vector Position
        {
            get => (Vector)GetValue(PositionProperty);
            set
            {
                SetValue(PositionProperty, value);
                if (_outerBorder != null) SetPos(new Vector(value.X, value.Y));
                if (Handles != null) UpdateHandles();
            }
        }
        private void SetPos(Vector v)
        {
            Canvas.SetLeft(_outerBorder, v.X - StrokeThickness);
            Canvas.SetTop(_outerBorder, v.Y - StrokeThickness);
            Canvas.SetLeft(this, v.X);
            Canvas.SetTop(this, v.Y);
            
        }

        // The minimum size to which the wrapper can be shrunk via dragging the handles
        public static readonly DependencyProperty ShrinkLimitProperty =
            DependencyProperty.Register(nameof(ShrinkLimit), typeof(double), typeof(ResizeWrapper), new PropertyMetadata(13.0));
        /// <summary>
        /// The minimum size in which the wrapper can be shrunk via dragging the handles.
        /// </summary>
        public double ShrinkLimit
        {
            get => (double)GetValue(ShrinkLimitProperty);
            set => SetValue(ShrinkLimitProperty, value);
        }

        //Whether to allow resizing through dragging drag-to-resize handles
        public static readonly DependencyProperty AllowResizeProperty =
            DependencyProperty.Register(nameof(AllowResize), typeof(bool), typeof(ResizeWrapper), new PropertyMetadata(false));
        /// <summary>
        /// Set true to allow resizing through dragging handles, set false to disable(true by default)
        /// </summary>
        public bool AllowResize
        {
            get => (bool)GetValue(AllowResizeProperty);
            set
            {
                if(value) EnableHandle();
                else DisableHandle();
                SetValue(AllowResizeProperty, value);
            }
        }

        //Whether to allow dragging border to move
        public static readonly DependencyProperty AllowDragProperty =
            DependencyProperty.Register(nameof(AllowDrag), typeof(bool), typeof(ResizeWrapper), new PropertyMetadata(true));
        /// <summary>
        /// Set true to allow dragging border to move, set false to disable(true by default)
        /// </summary>
        public bool AllowDrag
        {
            get => (bool)GetValue(AllowDragProperty);
            set
            {
                if(value) SubscribeBorder();
                else UnsubscribeBorder();
                SetValue(AllowDragProperty, value);
            }
        }

        public static readonly DependencyProperty UseSelectionProperty =
            DependencyProperty.Register(nameof(UseSelection), typeof(bool), typeof(ResizeWrapper), new PropertyMetadata(true));
        /// <summary>
        /// Set true to use selection, set false to disable(true by default)
        /// </summary>
        public bool UseSelection
        {
            get => (bool)GetValue(UseSelectionProperty);
            set
            {
                if (value)
                {

                    AllowResize = false;
                    AllowDrag = true;
                }
                else
                {
                    WrapperSelection.DeSelect(this);
                    AllowResize = true;
                    AllowDrag = true;
                }
                SetValue(UseSelectionProperty, value);
            }
        }


        // DependencyProperty for NestedResize
        public static readonly DependencyProperty NestedResizeProperty =
            DependencyProperty.Register(nameof(NestedResize), typeof(NestedResize), typeof(ResizeWrapper), new PropertyMetadata(NestedResize.Scale));

        /// <summary>
        /// How to adjust a ResizeWrapper located inside this ResizeWrapper.<br/>
        /// Set to None to apply no changes to size and relative position of nested ResizeWrappers<br/>
        /// Set to Scale to maintain proportion of relative position and size<br/>
        /// Set to MoveTrim to keep nested ResizeWrapper within boundary, and trimming if necessary(Unimplemented)
        /// </summary>
        public NestedResize NestedResize
        {
            get => (NestedResize)GetValue(NestedResizeProperty);
            set
            {
                if (value == NestedResize.MoveTrim)
                    throw new NotImplementedException("MoveTrim is not implemented yet!");
                SetValue(NestedResizeProperty, value);
            }
        }

        //The color of the surrounding Border
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ResizeWrapper), new PropertyMetadata(Brushes.Black));
        /// <summary>
        /// The color of the surrounding borderline
        /// </summary>
        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        //The stroke thickness of surrounding Border
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(ResizeWrapper), new PropertyMetadata(4.0));
        /// <summary>
        /// The Thickness value of the surrounding borderline
        /// </summary>
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set 
            {
                SetValue(StrokeThicknessProperty, value);
                InitializeOuterBorder();
            }
        }

        //Whether to enable rotational handles and rotation
        public static readonly DependencyProperty RotationEnabledProperty =
            DependencyProperty.Register(nameof(RotationEnabled), typeof(bool), typeof(ResizeWrapper), new PropertyMetadata(false));
        /// <summary>
        /// Set true to enable rotational handles and rotation, set false to disable(false by default)
        /// </summary>
        public bool RotationEnabled
        {
            get => (bool)GetValue(RotationEnabledProperty);
            set
            {
                if (value) throw new NotImplementedException("Rotations are not implemented yet!");
                SetValue(RotationEnabledProperty, value);
                setRotationProperty(value, RotationOffset);
            }
        }


        //The shape of the resize/rotational handles(Circle/Square)
        public static readonly DependencyProperty ShapeProperty =
            DependencyProperty.Register(nameof(HandleShape), typeof(ShapeType), typeof(ResizeWrapper), new PropertyMetadata(ShapeType.Circle));
        /// <summary>
        /// Property that describes the shape of the drag-to-resize/rotate handles (Circle / Square)
        /// </summary>
        public ShapeType HandleShape
        {
            get => (ShapeType)GetValue(ShapeProperty);
            set
            {
                SetValue(ShapeProperty, value);
                UpdateHandles();
            }
        }

        //Size of the handle
        public static readonly DependencyProperty HandleSizeProperty =
            DependencyProperty.Register(nameof(HandleSize), typeof(double), typeof(ResizeWrapper), new PropertyMetadata(10.0));
        /// <summary>
        /// The diameter/length/width of the handle, depending on the shape
        /// </summary>
        public double HandleSize
        {
            get => (double)GetValue(HandleSizeProperty);
            set
            {
                SetValue(HandleSizeProperty, value);
                UpdateHandles();
            }
        }

        //The color of the surrounding Border of the Handles
        public static readonly DependencyProperty HandleStrokeProperty =
            DependencyProperty.Register(nameof(HandleStroke), typeof(Brush), typeof(ResizeWrapper), new PropertyMetadata(Brushes.DarkGray));
        /// <summary>
        /// The color of the surrounding borderline of the drag-to-resize/rotate handles
        /// </summary>
        public Brush HandleStroke
        {
            get => (Brush)GetValue(HandleStrokeProperty);
            set
            {
                SetValue(HandleStrokeProperty, value);
                UpdateHandles();
            }
        }

        //The fill color of the Handles
        public static readonly DependencyProperty HandleFillProperty =
            DependencyProperty.Register(nameof(HandleFill), typeof(Brush), typeof(ResizeWrapper), new PropertyMetadata(Brushes.White));
        /// <summary>
        /// The fill color of the drag-to-resize/rotate handles
        /// </summary>
        public Brush HandleFill
        {
            get => (Brush)GetValue(HandleFillProperty);
            set
            {
                SetValue(HandleFillProperty, value);
                UpdateHandles(); // If you need to update handles when Fill is changed
            }
        }

        //The stroke thickness of handles
        public static readonly DependencyProperty HandleStrokeThicknessProperty =
            DependencyProperty.Register(nameof(HandleStrokeThickness), typeof(double), typeof(ResizeWrapper), new PropertyMetadata(1.5));
        /// <summary>
        /// The Thickness value of the surrounding borderline of the drag-to-resize/rotate handles
        /// </summary>
        public double HandleStrokeThickness
        {
            get => (double)GetValue(HandleStrokeThicknessProperty);
            set
            {
                SetValue(HandleStrokeThicknessProperty, value);
                UpdateHandles();
            }
        }

        //Vertical offset of rotational handle
        public static readonly DependencyProperty RotationOffsetProperty =
            DependencyProperty.Register(nameof(RotationOffset), typeof(double), typeof(ResizeWrapper), new PropertyMetadata(30.0));
        /// <summary>
        /// The vertical offset of the rotational handle
        /// </summary>
        public double RotationOffset
        {
            get => (double)GetValue(RotationOffsetProperty);
            set
            {
                SetValue(RotationOffsetProperty, value);
                setRotationProperty(RotationEnabled, value);
            }
        }
        #endregion

        #region Internal Controls
        internal ResizeHandle?[,] Handles { get; private set; }
        internal RotationHandle RotationHandle { get; private set; }
        private Border _outerBorder;
        internal Canvas outerPanel { get; private set; }
        #endregion

        #region DragHandle Control Variables
        internal Vector? initialMousePos = null;
        internal Vector? FixedPoint = null;
        internal Vector? MovePoint = null;
        internal Vector? vectorFactor = null;
        internal Vector? relNW = null;
        internal Vector? relSE = null;
        internal int FixedPointChange = 0;
        #endregion

        private void BorderMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (UseSelection && WrapperSelection.WrapperMouseDown(this, e)) return;
            Window? window = Converters.WindowOperations.FindParentWindow(this);
            if (window != null)
            {
                window.MouseMove += WindowsMouseMove;
            }
            vectorFactor = new Vector(1, 1);
            FixedPointChange = 1;
            initialMousePos = new Vector(e.GetPosition(null).X, e.GetPosition(null).Y);
            FixedPoint = Position;
            MovePoint = Position + new Vector(Width, Height);
            //Debug.WriteLine($"{Name}: BorderDown");
            recursiveRelativeTransformSetter();
            e.Handled = true;
            if (UseSelection) WrapperSelection.SyncBorder(this);
        }
        private void DiagonalMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (UseSelection && WrapperSelection.WrapperMouseDown(this, e)) return;
            Window? window = Converters.WindowOperations.FindParentWindow(this);
            if (window != null)
            {
                window.MouseMove += WindowsMouseMove;
            }
            ResizeHandle? handle = sender as ResizeHandle;
            if (handle == null) throw new UnauthorizedAccessException("Diagonal Mouse Handler can only be triggered for diagonal mouse clicks!");
            vectorFactor = new Vector(1, 1);
            FixedPointChange = 0;
            initialMousePos = new Vector(e.GetPosition(null).X, e.GetPosition(null).Y);
            Vector MidPoint = Position + new Vector(Width / 2, Height / 2);
            MovePoint = MidPoint + (handle.MovType * new Vector(Width / 2, Height / 2));
            FixedPoint = MidPoint + (handle.MovType * new Vector(Width / 2, Height / 2)) * (-1);
            //Debug.WriteLine($"{Name}: DiagonalDown");
            recursiveRelativeTransformSetter();
            e.Handled = true;
            if (UseSelection) WrapperSelection.SyncDiagonal(this, handle.MovType);
        }
        private void LinearMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (UseSelection && WrapperSelection.WrapperMouseDown(this, e)) return;
            Window? window = Converters.WindowOperations.FindParentWindow(this);
            if (window != null)
            {
                window.MouseMove += WindowsMouseMove;
            }
            ResizeHandle? handle = sender as ResizeHandle;
            if (handle == null) throw new UnauthorizedAccessException("Linear Mouse Handler can only be triggered for linear mouse clicks!");
            vectorFactor = new Vector(handle.MovType.X == 0 ? 0 : 1, handle.MovType.Y == 0 ? 0 : 1);
            FixedPointChange = 0;
            initialMousePos = new Vector(e.GetPosition(null).X, e.GetPosition(null).Y);
            Vector MidPoint = Position + new Vector(Width / 2, Height / 2);
            Vector RotateVector = new Vector(handle.MovType.X - handle.MovType.Y, handle.MovType.X + handle.MovType.Y); //rotates as if clockwise+1 rel to original handle
            MovePoint = MidPoint + (RotateVector * new Vector(Width / 2, Height / 2));
            FixedPoint = MidPoint + (RotateVector * new Vector(Width / 2, Height / 2)) * (-1);
            recursiveRelativeTransformSetter();
            //Debug.WriteLine($"{Name}: LinearDown");
            e.Handled = true;
            if (UseSelection) WrapperSelection.SyncLinear(this, handle.MovType);
        }

        private void WindowsMouseMove(object sender, MouseEventArgs e)
        {
            if (FixedPoint != null && MovePoint != null && initialMousePos != null && vectorFactor != null)
            {
                //Debug.WriteLine($"{Name}: Mov");
                Vector Movement = new Vector(e.GetPosition(null).X, e.GetPosition(null).Y) - initialMousePos;
                //Debug.WriteLine($"Movement: {Movement.X},{Movement.Y}");
                Vector a = Movement * vectorFactor + MovePoint;
                Vector b = Movement * FixedPointChange + FixedPoint;
                //Debug.WriteLine($"MP: {a.X},{a.Y}");
                //Debug.WriteLine($"FP: {b.X},{b.Y}");
                Vector prevsize = ResizeFromVectors(Movement * vectorFactor + MovePoint, Movement * FixedPointChange + FixedPoint);
                e.Handled = true;
                if (UseSelection) WrapperSelection.SyncTransform(this, Movement, prevsize);
            }
        }
        
        private void WindowsMouseUp(object sender, MouseEventArgs e)
        {
            Window? window = Converters.WindowOperations.FindParentWindow(this);
            if (window != null)
            {
                window.MouseMove -= WindowsMouseMove;
            }
            initialMousePos = null;
            FixedPoint = null;
            MovePoint = null;
            vectorFactor = null;
            relNW = null;
            relSE = null;
            FixedPointChange = 0;
            //Debug.WriteLine($"{Name}: Up");
            if (UseSelection) WrapperSelection.SyncMouseUp(this);
        }

        internal Vector ResizeFromVectors(Vector MovePoint, Vector FixPoint)
        {
            double Min = ShrinkLimit;
            double baseX = Math.Min(MovePoint.X, FixPoint.X);
            double baseY = Math.Min(MovePoint.Y, FixPoint.Y);
            double topX = Math.Max(MovePoint.X, FixPoint.X);
            double topY = Math.Max(MovePoint.Y, FixPoint.Y);
            double pWidth = topX - baseX;
            double pHeight = topY - baseY;
            double prevWidth, prevHeight;

            if (pWidth < Min || pHeight < Min)
            {
                Vector newMovePoint = new Vector(0, 0);
                if (pWidth < Min) newMovePoint.X = ((MovePoint.X - FixPoint.X) < 0 ? -1 : 1) * Min + FixPoint.X;
                else newMovePoint.X = MovePoint.X;
                if (pHeight < Min) newMovePoint.Y = ((MovePoint.Y - FixPoint.Y) < 0 ? -1 : 1) * Min + FixPoint.Y;
                else newMovePoint.Y = MovePoint.Y;
                baseX = Math.Min(newMovePoint.X, FixPoint.X);
                baseY = Math.Min(newMovePoint.Y, FixPoint.Y);
                topX = Math.Max(newMovePoint.X, FixPoint.X);
                topY = Math.Max(newMovePoint.Y, FixPoint.Y);
                pWidth = topX - baseX;
                pHeight = topY - baseY;
            }
            prevWidth = Width;
            prevHeight = Height;
            Vector newposition = new Vector(baseX, baseY);

            TriggerResizeEvent(Width, Height, pWidth, pHeight, Position, newposition);

            Position = newposition;
            Width = pWidth;
            Height = pHeight;
            InitializeOuterBorder();
            UpdateHandles();

            if (NestedResize == NestedResize.Scale)
            {
                foreach (UIElement element in Children)
                {
                    if (ComponentProperties.GetType(element) == ComponentType.ResizeWrapperBorder && element is Border wrapborder)
                    {
                        UIElement c = wrapborder.Child;
                        if (c is ResizeWrapper nestedChild && nestedChild.relNW != null && nestedChild.relSE != null)
                        {
                            nestedChild.ResizeFromVectors(
                            nestedChild.relNW * new Vector(Width, Height),
                            nestedChild.relSE * new Vector(Width, Height));
                        }
                    }
                }
            }
            return new Vector(prevWidth, prevHeight);
        }

        internal void recursiveRelativeTransformSetter()
        {
            if (NestedResize == NestedResize.Scale)
            {
                foreach (UIElement element in Children)
                {
                    if (ComponentProperties.GetType(element) == ComponentType.ResizeWrapperBorder && element is Border wrapborder)
                    {
                        UIElement c = wrapborder.Child;
                        if (c is ResizeWrapper nestedChild)
                        {
                            nestedChild.relNW = nestedChild.Position / new Vector(Width, Height);
                            nestedChild.relSE = (nestedChild.Position + new Vector(nestedChild.Width, nestedChild.Height)) / new Vector(Width, Height);
                        }
                    }
                }
            }
        }

        public void ForceResize(Vector Point1, Vector Point2)
        {
            double baseX = Math.Min(Point1.X, Point2.X);
            double baseY = Math.Min(Point1.Y, Point2.Y);
            double topX = Math.Max(Point1.X, Point2.X);
            double topY = Math.Max(Point1.Y, Point2.Y);
            double pWidth = topX - baseX;
            double pHeight = topY - baseY;


            Position = new Vector(baseX, baseY);
            Width = pWidth;
            Height = pHeight;
            InitializeOuterBorder();
            UpdateHandles();
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is FrameworkElement child) // Using pattern matching for concise type checking and casting
            {
                if (ComponentProperties.GetType(child) == ComponentType.None)
                {
                    // If child is a valid external control
                    child.Loaded += (s, e) =>
                    {
                        child.Width = Width;
                        child.Height = Height;
                        SetLeft(child, 0);
                        SetTop(child, 0);
                    };

                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            foreach (UIElement element in Children) {
                if (element is FrameworkElement child) // Using pattern matching for concise type checking and casting
                {
                    if (ComponentProperties.GetType(child) == ComponentType.None)
                    {
                        // If child is a valid external control
                        child.Width = sizeInfo.NewSize.Width;
                        child.Height = sizeInfo.NewSize.Height;
                        SetLeft(child, 0);
                        SetTop(child, 0);
                    }
                }
            }
        }

        /// <summary>
        /// A event triggered when element is resized or moved by mouse movement
        /// </summary>
        public event EventHandler<DimensionChangedEventArgs> SizeChangedByDrag;

        private void TriggerResizeEvent(double prevWidth, double prevHeight, double Width, double Height, Vector prevLoc, Vector newLoc)
        {
            SizeChangedByDrag?.Invoke(this,
                new DimensionChangedEventArgs(new Vector(prevWidth, prevHeight), new Vector(Width, Height), prevLoc, newLoc));
        }

        public event EventHandler<SelectionEventArgs> OnDeSelected;
        public event EventHandler<SelectionEventArgs> OnSelected;

        internal void TriggerSelect(System.Collections.Generic.List<ResizeWrapper> prev, System.Collections.Generic.List<ResizeWrapper> next)
        {
            OnSelected.Invoke(this, new SelectionEventArgs(prev, next));
        }

        internal void TriggerDeSelect(System.Collections.Generic.List<ResizeWrapper> prev, System.Collections.Generic.List<ResizeWrapper> next)
        {
            OnDeSelected.Invoke(this, new SelectionEventArgs(prev, next));
        }

    }
    public enum NestedResize
    {
        None,
        Scale,
        MoveTrim
    }
}
