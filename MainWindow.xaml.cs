using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;


namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public static string fsn = fileName.Substring(0, fileName.LastIndexOf('.')) + "Config.txt";
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (File.Exists(fsn))
            {
                FileStream fs = new FileStream(fsn, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string s = sr.ReadToEnd().Split("AutoStart=")[1];
                sr.Close();
                fs.Close();
                if (s.Equals("Y"))
                {
                    BT.Content = "关闭自启";
                }
                else if (s.Equals("N"))
                {
                    BT.Content = "开机自启";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            bool TF = false;
           
            if (File.Exists(fsn))
            {
                FileStream fs = new FileStream(fsn, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string s = sr.ReadToEnd().Split("AutoStart=")[1];
                sr.Close();
                fs.Close();
                if (s.Equals("Y"))
                {
                    TF = false;
                }else if (s.Equals("N"))
                {
                    TF = true;
                }
                TBK.Text = WriteRegistry(fileName, TF);

                fs = new FileStream(fsn, FileMode.Open);
                sr = new StreamReader(fs);
                s = sr.ReadToEnd().Split("AutoStart=")[1];
                if (s.Equals("Y"))
                {
                    BT.Content = "关闭自启";
                }
                else if (s.Equals("N"))
                {
                    BT.Content = "开机自启";
                }
                sr.Close();
                fs.Close();
                return;
            }
            TBK.Text =  WriteRegistry(fileName,TF);

        }

        private static string WriteRegistry(string strName,bool onOFF)
        {
            if (File.Exists(strName))
            {
                FileStream fs = new FileStream(fsn, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                try
                {
                   
                    string strNewName = System.IO.Path.GetFileName(strName).Split('.')[0];
                    RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (onOFF)
                    {
                        if (reg.GetValue(strNewName) == null)
                        {
                            reg.SetValue(strNewName, strName);
                            sw.Write("AutoStart=Y");
                            sw.Flush();

                        }
                        return "已自启";
                    }
                    else
                    {
                        if (reg.GetValue(strNewName) != null)
                        {
                            reg.DeleteValue(strNewName);
                            sw.Write("AutoStart=N");
                            sw.Flush();
                        }
                        return "已关闭";
                    }
                }catch(Exception e)
                {
                    return e.StackTrace;
                }
                finally
                {
                    sw.Close();
                    fs.Close();
                }
            }
            else
            {
                return "没有此文件";
            }
        }
    }
}
