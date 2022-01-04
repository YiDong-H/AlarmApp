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
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public static string fsn = fileName.Substring(0, fileName.LastIndexOf("\\")) + "\\Config.txt";
        public static string studyRecord = fileName.Substring(0, fileName.LastIndexOf("\\")) + "\\StudyRecord.txt";
        private TimeSpan ts = new TimeSpan();
        DateTime dts = new DateTime();
        DispatcherTimer timer = new DispatcherTimer();
        static string content = "";
        public MainWindow()
        {

            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (File.Exists(fsn))
            {
                string s = ReadConfig("AutoStart",0);
                if (s.Equals("Y"))
                {
                    BT.Content = "关闭自启";
                }
                else if (s.Equals("N"))
                {
                    BT.Content = "开机自启";
                }
            }
            else if (File.Exists(fileName))
            {
                FileStream fs = new FileStream(fsn, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write("AutoStart=N;");
                sw.Write("StudyStart=N;");
                sw.Flush();
                sw.Close();
                fs.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            bool TF = false;
           
            if (File.Exists(fsn))
            {
                string s = ReadConfig("AutoStart", 0);
                if (s.Equals("Y"))
                {
                    TF = false;
                }else if (s.Equals("N"))
                {
                    TF = true;
                }
                TBK.Text = WriteRegistry(fileName, TF);

                s = ReadConfig("AutoStart", 0);
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
            TBK.Text =  WriteRegistry(fileName,TF);

        }

        private static string WriteRegistry(string strName,bool onOFF)
        {
            if (File.Exists(strName))
            {
                try
                {
                   
                    string strNewName = System.IO.Path.GetFileName(strName).Split('.')[0];
                    RegistryKey reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (onOFF)
                    {
                        if (reg.GetValue(strNewName) == null)
                        {
                            reg.SetValue(strNewName, strName);
                            ChangeConfig("AutoStart", 10, 'Y');

                        }
                        else
                        {
                            ChangeConfig("AutoStart", 10, 'Y');
                        }
                        return "已自启";
                    }
                    else
                    {
                        if (reg.GetValue(strNewName) != null)
                        {
                            reg.DeleteValue(strNewName);
                            ChangeConfig("AutoStart", 10, 'N');
                        }
                        else
                        {
                            ChangeConfig("AutoStart", 10, 'N');
                        }
                        return "已关闭";
                    }
                }catch(Exception e)
                {
                    return e.StackTrace;
                }
            }
            else
            {
                return "此文件夹下没有找到app";
            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string s = ReadConfig("StudyStart", 1);
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            if (s.Equals("N"))
            {
                StudyContent st = new StudyContent();
                st.accept += delegate { content = (string)sender; };
                //Hide();
                st.Topmost = true;
                st.ShowDialog();
                if(st.DialogResult == true)
                {
                    Show();
                    if (File.Exists(fsn))
                    {
                        ChangeConfig("StudyStart", 11, 'Y');
                    }
                    StudyR.Content = "Stop";
                    dts = DateTime.Now;
                    timer.Start();
                }
                else
                {
                    Show();
                }
                
            }else if (s.Equals("Y"))
            {
                timer.Stop();
                ChangeConfig("StudyStart", 11, 'N');
                string record = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                StudyR.Content = "Start";
                timel.Content = "00:00:00";
                if (File.Exists(fileName))
                {
                    FileStream fs = new FileStream(studyRecord, FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(DateTime.Now.ToShortDateString() + "          学习内容："+content+ "          学习时长：" + record);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
                content = "";
            }
            
        }


        public void Timer_Tick(object sender, EventArgs e)
        {
            ts = DateTime.Now - dts;
            timel.Content = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }


        public static void ChangeConfig(string name,int index,char value)
        {
            FileStream fs = new FileStream(fsn, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string s = sr.ReadToEnd();
            sr.Close();
            FileStream fs2 = new FileStream(fsn, FileMode.Open, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs2);

            StringBuilder sb = new StringBuilder(s);
            try
            {
                sb[s.IndexOf(name+"=") + index] = value;
                sw.Write(sb.ToString());
                sw.Flush();
            }
            catch (Exception ex)
            {
                return;
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }

        public static string ReadConfig(string name,int locate)
        {
            FileStream fs = new FileStream(fsn, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string sc = sr.ReadToEnd().Split(';')[locate];
            string s = sc.Split(name+"=")[1];
            sr.Close();
            fs.Close();
            return s;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ChangeConfig("StudyStart", 11, 'N');
            base.OnClosing(e);
        }

        public static void setContent(string value)
        {
            content = value;
        }
    }
}
