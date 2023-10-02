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
    /// Interaction logic for ScreenshotSave.xaml
    /// </summary>
    public partial class ScreenshotSave : Window
    {
        public FileItem SelectedFileItem { get; set; }

        public ScreenshotSave()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TitleTB.Text == "" || DescriptionTB.Text == "")
            {
                MessageBox.Show("입력하지 않은 항목이 있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                SelectedFileItem = new FileItem { Filename = TitleTB.Text, Description = DescriptionTB.Text };
                this.DialogResult = true;
            }
        }

    }
}
