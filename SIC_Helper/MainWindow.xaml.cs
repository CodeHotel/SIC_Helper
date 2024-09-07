using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;


namespace SIC_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        CaptureWindow capturewindow;
        IntPtr DOSBoxWindow = IntPtr.Zero;
        public SaveFile file;
        public FileViewModel fileViewModel;
        private BitmapSource tempImg;
        public MainWindow()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filePath = System.IO.Path.Combine(appDataPath, "UmJunSIC", "UserData");

            if (System.IO.File.Exists(filePath))
            {
                file = SaveFile.Deserialize(filePath);
            }

            this.DataContext = new FileViewModel();
            InitializeComponent();
            capturewindow = new CaptureWindow();
            capturewindow.parentReference = this;
            capturewindow.Show();
            capturewindow.Closing += onRecClose;
        }


        private void onRecClose(object sender, CancelEventArgs e)
        {
            capturewindow.Closing -= onRecClose;
            capturewindow = new CaptureWindow();
            capturewindow.parentReference = this;
            capturewindow.Show();
            capturewindow.Closing += onRecClose;
        }

        private async void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            await initScreenshot(sender, e);
            if (tempImg != null)
            {
                FileItem? returnedFileItem = null;
                bool existsFlag = true;
                while (existsFlag)
                {
                    bool calc = true;

                    var dialogWindow = new ScreenshotSave();
                    if (dialogWindow.ShowDialog() == true)
                    {
                        returnedFileItem = dialogWindow.SelectedFileItem;
                    }
                    foreach(FileItem item in fileViewModel.FileItems)
                    {
                        if (item.Filename == returnedFileItem.Filename)
                        {
                            calc = false;
                            MessageBox.Show("중복되는 파일 이름입니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            calc &= true;
                        }
                    }
                    if (!calc) existsFlag = true;
                    else existsFlag = false;
                }
                if (returnedFileItem != null)
                {
                    string path = System.IO.Path.Combine(TB0.Text, returnedFileItem.Filename + ".png");
                    if (!Directory.Exists(TB0.Text))
                    {
                        MessageBox.Show("올바른 폴더를 설정해 주십시오", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(tempImg));
                        encoder.Save(fileStream);
                    }

                    fileViewModel.FileItems.Add(returnedFileItem);
                    if (!fileViewModel.SaveToPath(TB0.Text))
                    {
                        MessageBox.Show("메타데이터 저장 오류", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private async Task initScreenshot(object sender, RoutedEventArgs e)
        {
            Rectangle area = capturewindow.CaptureArea;

            if (area != null)
            {
                await sccap(area);
            }
        }

        private async Task sccap(Rectangle captureRect)
        {
            BitmapSource bmpsource = null;
            double x = captureRect.PointFromScreen(new System.Windows.Point(0d, 0d)).X;
            double y = captureRect.PointFromScreen(new System.Windows.Point(0d, 0d)).Y;

            Point DPI = CurrentWindowFindDpi();
            double DpiScaleX = DPI.X;
            double DpiScaleY = DPI.Y;

            var wpfActiveScreen = System.Windows.SystemParameters.WorkArea;
            System.Drawing.Bitmap localBmp = new System.Drawing.Bitmap((int)(captureRect.Width * DpiScaleX), (int)(captureRect.Height * DpiScaleY), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr bmptr;

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(localBmp))
            {
                g.CopyFromScreen((int)((wpfActiveScreen.X - x) * DpiScaleX), (int)((wpfActiveScreen.Y - y) * DpiScaleY), 0, 0, localBmp.Size, System.Drawing.CopyPixelOperation.SourceCopy);

                bmptr = localBmp.GetHbitmap();
                bmpsource = Imaging.CreateBitmapSourceFromHBitmap(bmptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(bmptr);  // Don't forget to delete the object.
            }

            // Use bmpsource as needed, for example, assign it to an Image control's Source property
            tempImg = bmpsource;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr hObject);

        private Point CurrentWindowFindDpi()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            double dpiX = 1; double dpiY = 1;
            if (source != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }
            double DpiScaleX = Math.Round(dpiX / (double)96, 2);
            double DpiScaleY = Math.Round(dpiY / (double)96, 2);
            return new Point(DpiScaleX, DpiScaleY);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            IntPtr hWnd = IntPtr.Zero;
            while (true)
            {
                hWnd = FindWindowEx(IntPtr.Zero, hWnd, null, null);
                if (hWnd == IntPtr.Zero) break; // no more windows

                const int nChars = 256;
                StringBuilder Buff = new StringBuilder(nChars);
                if (GetWindowText(hWnd, Buff, nChars) > 0)
                {
                    string windowTitle = Buff.ToString();
                    if (windowTitle.StartsWith("DOSBox 0."))
                    {
                        // Handle found
                        break;
                    }
                }
            }

            if (hWnd != IntPtr.Zero)
            {
                DOSBoxWindow = hWnd;
                DosStatus.Foreground = Brushes.LightGreen;
                DosStatus.Content = "검색됨";
            }
            else
            {
                DOSBoxWindow = IntPtr.Zero;
                DosStatus.Foreground = Brushes.Red;
                DosStatus.Content = "검색되지 않음";
            }
        }

        private async void macro1(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB1.Text);
        }

        private async void macro2(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB2.Text);
        }

        private async void macro3(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB3.Text);
        }

        private async void macro4(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB4.Text);
        }

        private async void macro5(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB5.Text);
        }

        private async void macro7(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB7.Text);
        }

        private async void macro8(object sender, RoutedEventArgs e)
        {
            await SendKeyboardInput(TB8.Text);
        }

        private async Task BringDosTop()
        {
            await Task.Run(async () =>
            {
                if (DOSBoxWindow != IntPtr.Zero)
                {
                    // Bring DOSBox window to the front
                    if (!SetForegroundWindow(DOSBoxWindow))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("DOSBox를 최상단으로 끌어오기에 실패하였습니다", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                        DOSBoxWindow = IntPtr.Zero;
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("DOSBox가 켜져 있지 않거나 동기화가 되어 있지 않습니다", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
            });
        }

        private async Task SendKeyboardInput(string input)
        {
            // return if the string does not comply with the rules
            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9\.,\\ $\-:]+$"))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("허용되지 않은 문자 발견(a~z, A~Z, 0~9, $(엔터), -, ',', \\만 사용 가능)", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
                return;
            }
            await BringDosTop();
            if (DOSBoxWindow == IntPtr.Zero) return;

            var simulator = new InputSimulator();
            string[] commands = input.Split('$');

            for (int i = 0; i < commands.Length; i++)
            {
                var command = commands[i];
                foreach (var character in command)
                {
                    VirtualKeyCode vkCode;
                    switch (character)
                    {
                        case '.':
                            vkCode = VirtualKeyCode.OEM_PERIOD;
                            break;
                        case '-':
                            vkCode = VirtualKeyCode.OEM_MINUS;
                            break;
                        case ' ':
                            vkCode = VirtualKeyCode.SPACE;
                            break;
                        case ',':
                            vkCode = VirtualKeyCode.OEM_COMMA;
                            break;
                        case ':':
                            vkCode = VirtualKeyCode.OEM_1;
                            break;
                        case '\\':
                            vkCode = VirtualKeyCode.OEM_5;
                            break;
                        default: // For alphanumeric characters
                            vkCode = (VirtualKeyCode)Enum.Parse(typeof(VirtualKeyCode), "VK_" + character.ToString().ToUpper());
                            break;
                    }
                    if(vkCode == VirtualKeyCode.OEM_1)
                    {
                        simulator.Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
                        await Task.Delay(10);
                    }
                    simulator.Keyboard.KeyDown(vkCode);
                    await Task.Delay(10);
                    simulator.Keyboard.KeyUp(vkCode);
                    await Task.Delay(10);
                    if (vkCode == VirtualKeyCode.OEM_1)
                    {
                        simulator.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
                    }
                }// if it's not the last command in the array, then press Enter.
                if (i < commands.Length - 1)
                    simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                await Task.Delay(100);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveFile s = new SaveFile();
            s.String1 = TB0.Text;
            s.String2 = TB1.Text;
            s.String3 = TB2.Text;
            s.String4 = TB3.Text;
            s.String5 = TB4.Text;
            s.String6 = TB5.Text;
            s.String7 = TB7.Text;
            s.String8 = TB8.Text;
            s.recPos = capturewindow.recordRect.Position;
            s.recSize = new DrawNet_WPF.Converters.Vector(capturewindow.recordRect.Width, capturewindow.recordRect.Height);
            s.windowPos = new DrawNet_WPF.Converters.Vector(Left, Top);
            s.SaveUserData();
            capturewindow.Closing -= onRecClose;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                capturewindow?.Dispatcher.Invoke(() => capturewindow.WindowState = WindowState.Minimized);
            }
            else
            {
                capturewindow?.Dispatcher.Invoke(() => capturewindow.WindowState = WindowState.Maximized);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (file != null)
            {
                TB0.Text = file.String1;
                TB1.Text = file.String2;
                TB2.Text = file.String3;
                TB3.Text = file.String4;
                TB4.Text = file.String5;
                TB5.Text = file.String6;
                TB7.Text = file.String7;
                TB8.Text = file.String8;
                Left = file.windowPos.X; 
                Top = file.windowPos.Y;
            }
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.ValidateNames = false;
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.FileName = "Folder Selection.";
            dialog.Filter = "Folders|no.files";

            if (dialog.ShowDialog() == true)
            {
                string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                TB0.Text = folderPath;
            }
        }

        private void TB0_TextChanged(object sender, TextChangedEventArgs e)
        {
            fileViewModel = FileViewModel.LoadFromPath(TB0.Text);
            this.DataContext = fileViewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem is FileItem selectedFile)
            {
                string path = System.IO.Path.Combine(TB0.Text, selectedFile.Filename);
                path += ".png";
                if(File.Exists(path))
                {
                    displayImage.Source = new BitmapImage(new Uri(path));
                }
                else
                {
                    fileViewModel.Verify(TB0.Text);
                }
            }
        }

        private void WinCloseButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }

}

