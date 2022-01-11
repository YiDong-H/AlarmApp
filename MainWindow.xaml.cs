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
using System.Text.RegularExpressions;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static string fileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        public static string fsn = "C:\\AlarmApp\\Config\\Config.txt";
        public static string settingImg = fileName.Substring(0, fileName.LastIndexOf("\\")) + "\\img\\Setting.png";
        public static string studyRecord = fileName.Substring(0, fileName.LastIndexOf("\\")) + "\\StudyRecord.txt";
        private TimeSpan ts = new TimeSpan();
        DateTime dts = new DateTime();
        DateTime nowt;
        DispatcherTimer timer = new DispatcherTimer();
        static string content = "";
        static string alarmText = "";
        static string bkImg = "";
        LocalConfig localConfig;

        public static string TempAlarmText { get => alarmText; set => alarmText = value; }
        public static string TempBkImg { get => bkImg; set => bkImg = value; }

        public MainWindow()
        {

            InitializeComponent();
            if(!Directory.Exists("C:\\AlarmApp\\img"))
            {
                Directory.CreateDirectory("C:\\AlarmApp\\img");
            }
            if (!Directory.Exists("C:\\AlarmApp\\Config"))
            {
                Directory.CreateDirectory("C:\\AlarmApp\\Config");
            }
            if (!File.Exists("C:\\AlarmApp\\img\\Setting.png"))
            {
                if (File.Exists(settingImg))
                {
                    File.Copy(settingImg, "C:\\AlarmApp\\img\\Setting.png");
                }
                else
                {
                    MessageBox.Show("缺少配置文件，请重新下载软件");
                    return;
                }
            }
            localConfig = new LocalConfig();
            settingImg = "C:\\AlarmApp\\img\\Setting.png";
            localConfig.SetImg = settingImg;
            this.DataContext = localConfig;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (File.Exists(fileName)&&!File.Exists(fsn))
            {
                FileStream fs = new FileStream(fsn, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs,Encoding.UTF8);
                sw.WriteLine("AutoStart=N;");
                sw.WriteLine("AlarmNote=在(左上角)设置中设置需要的文本~;");
                sw.WriteLine("BackgroundImageSource=\"\";");
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            string pattern = @"(?:\.jpg|\.gif|\.png|\.jpeg)$";
            string bkimg = ReadConfig("BackgroundImageSource", 2);
            bool match=Regex.IsMatch(bkimg,pattern);
            localConfig.AlarmText = ReadConfig("AlarmNote", 1);
            if (match)
            {
                if (File.Exists(bkimg))
                {
                    localConfig.BkImg = bkimg;
                }
                else
                {
                    localConfig.BkImg = "";
                }
                
            }
            else
            {
                localConfig.BkImg = "";
            }
            
        }

        //开启/关闭开机自启
        public static string WriteRegistry(string strName,bool onOFF)
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

        //Start/Stop按钮
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            if (localConfig.StudyRecordStart.Equals("Start "))
            {
                StudyContent st = new StudyContent();
                st.Topmost = true;
                st.ShowDialog();
                if(st.DialogResult == true)
                {
                    Show();
                    localConfig.StudyRecordStart = "Stop ";
                    dts = DateTime.Now;
                    nowt = dts;
                    timer.Start();
                }
                else
                {
                    Show();
                }
                
            }else if (localConfig.StudyRecordStart.Equals("Stop "))
            {
                timer.Stop();
                string record = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
                localConfig.StudyRecordStart = "Start";
                timel.Content = "00:00:00";
                if (File.Exists(fileName))
                {
                    FileStream fs = new FileStream(studyRecord, FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine();
                    sw.WriteLine(DateTime.Now.ToString() + "\r学习内容：" + content+ "\r学习时长：" + record);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
                MessageBox.Show("\t保存成功!\r\t学习内容："+content+ "\r\t学习时长：" + record);
                content = "";

            }
            
        }


        public void Timer_Tick(object sender, EventArgs e)
        {
            nowt= nowt.AddSeconds(1);
            ts = nowt - dts;
            timel.Content = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
        }


        public static void ChangeConfig(string name,int index,char value)
        {
            try
            {
                FileStream fs = new FileStream(fsn, FileMode.Open);
                StreamReader sr = new StreamReader(fs,Encoding.UTF8);
                string s = sr.ReadToEnd();
                sr.Close();
                fs.Close();

                FileStream fs2 = new FileStream(fsn, FileMode.Open, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs2, Encoding.UTF8);

                StringBuilder sb = new StringBuilder(s);
            
                sb[s.IndexOf(name + "=") + index] = value;
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败");
                return;
            }
        }

        public static void ChangeConfig(string name, string oldvalue,string value)
        {
            try
            {
                FileStream fs = new FileStream(fsn, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                string s = sr.ReadToEnd();
                sr.Close();
                fs.Close();
                sr.Dispose();
                fs.Dispose();
                File.Delete(fsn);
                FileStream fs2 = new FileStream(fsn, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs2, Encoding.UTF8);
                StringBuilder sb = new StringBuilder(s);
                sb = sb.Replace(name + "=" + oldvalue,name+"="+value) ;
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
                fs2.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败");
                return;
            }
            finally
            {
                
            }
        }

        /// <summary>
        /// 读取配置中键的值
        /// </summary>
        /// <param name="name">键名</param>
        /// <param name="locate">第几个分号之前，从0开始</param>
        /// <returns></returns>
        public static string ReadConfig(string name,int locate)
        {
            try
            {
                FileStream fs = new FileStream(fsn, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.UTF8);
                string sc = sr.ReadToEnd().Split(';')[locate];
                string s = sc.Split(name + "=")[1];
                sr.Close();
                fs.Close();
                return s;
            }
            catch(FileNotFoundException fnex)
            {
                MessageBox.Show("未找到配置文件,重启软件重试！");
                return "";
            }
            
        }

        //关闭程序之前，保存一切配置
        protected override void OnClosing(CancelEventArgs e)
        {
            string oldAla = ReadConfig("AlarmNote", 1);
            string newAla = localConfig.AlarmText;
            ChangeConfig("AlarmNote", oldAla, newAla);

            string oldBkImgs = ReadConfig("BackgroundImageSource",2);
            string newBkImgs = localConfig.BkImg;
            ChangeConfig("BackgroundImageSource", oldBkImgs, newBkImgs);
            base.OnClosing(e);
        }

        public static void setContent(string value)
        {
            content = value;
        }

        //打开设置窗口
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TempAlarmText = localConfig.AlarmText;
            TempBkImg = localConfig.BkImg;
            Setting setting = new Setting();
            setting.Topmost = true;
            setting.ShowDialog();
            if (setting.DialogResult == true)
            {
                localConfig.AlarmText = TempAlarmText;
                localConfig.BkImg = TempBkImg;
            }
        }

    }

    public class LocalConfig : NotifyChange
    {
        private string _SetImg;
        private string _BkImg;
        private string _AlarmText;
        private string _StudyRecordStart="Start ";
        public string SetImg
        {
            get { return _SetImg; }
            set
            {
                SetProperty(ref _SetImg, value);
            }
        }

        public string BkImg
        {
            get { return _BkImg; }
            set
            {
                SetProperty(ref _BkImg, value);
            }
        }

        public string AlarmText
        {
            get { return _AlarmText; }
            set
            {
                SetProperty(ref _AlarmText, value);
            }
        }

        public string StudyRecordStart
        {
            get { return _StudyRecordStart; }
            set
            {
                SetProperty(ref _StudyRecordStart, value);
            }
        }
    }

}
