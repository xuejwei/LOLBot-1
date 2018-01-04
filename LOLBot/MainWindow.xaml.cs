using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MessageBox = System.Windows.MessageBox;

using Automation.DD;
using Automation.WinApi;

namespace LOLBot
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        WindowInteropHelper interopHelper;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            interopHelper = new WindowInteropHelper(this);
            //获得消息源
            HwndSource source = HwndSource.FromHwnd(interopHelper.Handle);
            //判断消息源是否有效
            if (null == source)
            {
                //如果消息源无效
                MessageBox.Show("绑定热键失败，快找我反馈 BUG");
            }
            else
            {
                //挂接事件
                source.AddHook(HotKeyHook);
                bool regF9 = User32.RegisterHotKey(interopHelper.Handle, 233, 0, (int)Keys.F9);
                if (!regF9)
                {
                    this.tipLabel.Content = "F9 热键注册失败";
                }
            }

            string errString = DDutil.init();
            if(errString != null)
            {
                MessageBox.Show(errString);
            }
        }

        private IntPtr HotKeyHook(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            switch (msg)
            {
                //热键消息
                case 0x0312:
                    if (wideParam.ToInt32() == 233)
                    {
                        //按了 F9
                        startBot();
                    }

                    if (wideParam.ToInt32() == 4843)
                    {
                        //按了 F10
                        stopBot();
                    }

                    break;

            }
            return IntPtr.Zero;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            startBot();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stopBot();
        }

        private void startBot()
        {
            Bot.Start();
            //注册终止的热键，并解绑其热键
            bool regF10 = User32.RegisterHotKey(interopHelper.Handle, 4843, 0, (int)Keys.F10);
            User32.UnregisterHotKey(interopHelper.Handle, 233);
            if (!regF10)
            {
                this.tipLabel.Content = "F10 热键注册失败";
            }
            

            this.startButton.IsEnabled = false;
            this.stopButton.IsEnabled = true;
        }

        private void stopBot()
        {
            Bot.Stop();
            //注册开始的热键，并解绑其他热键
            bool regF9 = User32.RegisterHotKey(interopHelper.Handle, 233, 0, (int)Keys.F9);
            User32.UnregisterHotKey(interopHelper.Handle, 4843);
            if (!regF9)
            {
                this.tipLabel.Content = "F9 热键注册失败";
            }

            this.startButton.IsEnabled = true;
            this.stopButton.IsEnabled = false;
        }

        
    }
}
