using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        public static string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public static string fsn = "C:\\AlarmApp\\Config\\Config.txt";
        BgImage bgi;
        public Setting()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            bgi = new BgImage();
            this.DataContext = bgi;
            if (File.Exists(fsn))
            {
                string s = MainWindow.ReadConfig("AutoStart", 0);
                if (s.Equals("Y"))
                {
                    BT.Content = "关闭自启";
                }
                else if (s.Equals("N"))
                {
                    BT.Content = "开机自启";
                }
            }
            else
            {
                MessageBox.Show("未找到配置文件，请重启软件");
                DialogResult = false;
                Close();

            }
            AlarmText.Text = MainWindow.TempAlarmText;
            bgi.Location = MainWindow.TempBkImg;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.TempAlarmText = AlarmText.Text;
            DialogResult = true;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BT_Click(object sender, RoutedEventArgs e)
        {


            bool TF = false;

            if (File.Exists(fsn))
            {
                string s = MainWindow.ReadConfig("AutoStart", 0);
                if (s.Equals("Y"))
                {
                    TF = false;
                }
                else if (s.Equals("N"))
                {
                    TF = true;
                }
                MainWindow.WriteRegistry(fileName, TF);

                s = MainWindow.ReadConfig("AutoStart", 0);
                if (s.Equals("Y"))
                {
                    BT.Content = "关闭自启";
                }
                else if (s.Equals("N"))
                {
                    BT.Content = "开机自启";
                }
                return;
            }
            MainWindow.WriteRegistry(fileName, TF);
        }

        //选择背景图片按钮按下事件
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "\"Image File(*.jpg,*.png,*.gif,*.jpeg)\"|*.jpg;*.png;*.gif;*.jpeg";
            Hide();
            if (dialog.ShowDialog()==true)
            {
                bgi.Location= dialog.FileName;
                MainWindow.TempBkImg= dialog.FileName;
                this.ShowDialog();
            }
            else
            {
                this.ShowDialog();
            }
            
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }

    public class BgImage : NotifyChange
    {
        private string _Location="";

        public string Location
        {
            get { return _Location; }
            set
            {
                SetProperty(ref _Location, value);
            }
        }


    }
}
