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

namespace TestApp
{
    /// <summary>
    /// StudyContent.xaml 的交互逻辑
    /// </summary>
    public partial class StudyContent : Window
    {
        public event EventHandler accept;
        public StudyContent()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void Commit(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            MainWindow.setContent(StuCon.Text);
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        
    }
}
