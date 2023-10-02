using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SIC_Helper
{
    /// <summary>
    /// Interaction logic for CaptureWindow.xaml
    /// </summary>
    public partial class CaptureWindow : Window
    {
        private MainWindow _parentReference;
        public MainWindow parentReference
        {
            get => _parentReference;
            set
            {
                _parentReference = value;
                _parentReference.Closing += onWindowClose;
            }
        }

        public CaptureWindow()
        {
            InitializeComponent();

            // Assuming you have configured the OnSelected and OnDeselected events in recordRect
            recordRect.OnSelected += RecordRect_OnSelected;
            recordRect.OnDeSelected += RecordRect_OnDeSelected;
        }

        private void RecordRect_OnSelected(object sender, EventArgs e)
        {
            // Set Window Background to half opaque white when selected
            this.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255)); // 128 is half opacity
        }

        private void RecordRect_OnDeSelected(object sender, EventArgs e)
        {
            // Set Window Background to Transparent when deselected
            this.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                parentReference?.Dispatcher.Invoke(() => parentReference.WindowState = WindowState.Normal);
            }
        }

        private void onWindowClose(object sender, EventArgs e)
        {
            recordRect.OnSelected -= RecordRect_OnSelected;
            recordRect.OnDeSelected -= RecordRect_OnDeSelected;
            _parentReference.Closing += onWindowClose;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(parentReference != null)
            {
                if (parentReference.file != null)
                {
                    recordRect.Position = parentReference.file.recPos;
                    recordRect.Width = parentReference.file.recSize.X;
                    recordRect.Height = parentReference.file.recSize.Y;
                }
            }
        }
    }
}
